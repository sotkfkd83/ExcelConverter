using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

using ExcelDataReader;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace ExcelConvert
{
    public class ExcelToJsonMaker
    {
        protected string m_srcPath { get; set; }
        protected string m_outJsonPath { get; set; }
        protected string m_outCsPath { get; set; }
        protected string m_srcExtention { get; set; }
        private Action<string> m_lcoaCallback = null;

        public ExcelToJsonMaker(string srcPath, string srcExtentions, Action<string> logCallback = null)
        {
            m_srcPath = srcPath;
            m_srcExtention = srcExtentions;
            m_outJsonPath = $@"{m_srcPath}\JSON";
            m_outCsPath = $@"{m_srcPath}\CS";
            m_lcoaCallback = logCallback;
        }

        public ExcelToJsonMaker(string srcPath, string srcExtentions, string outJsonPath, string outCsPath, Action<string> logCallback = null)
        {
            m_srcPath = srcPath;
            m_srcExtention = srcExtentions;
            m_outJsonPath = outJsonPath;
            m_outCsPath = outCsPath;
            m_lcoaCallback = logCallback;
        }

        string [] GetSourceFiles()
        {
            if (Directory.Exists(m_srcPath) == false)
            {
                return null;
            }

            return Directory.GetFiles(m_srcPath, @"*", SearchOption.AllDirectories).Where(x => x.EndsWith($".{m_srcExtention}")).ToArray();
        }

        public void Convert()
        {
            var files = GetSourceFiles();
            if (files == null || files.Length <= 0)
            {
                m_lcoaCallback?.Invoke($"{m_srcPath} 경로에 변환할 파일이 존재하지 않습니다.");
                return;
            }

            ExportJson(files);
        }

        public void ExportJson(params string [] files)
        {
            if (files == null || files.Length <= 0)
            {
                return;
            }

            int count = files.Length;
            for (int i = 0; i < count; ++i)
            {
                var path = files[i];

                using (var reader = ExcelReaderFactory.CreateReader(File.Open(path, FileMode.Open, FileAccess.Read)))
                {
                    StringBuilder builder = new StringBuilder();

                    var result = reader.AsDataSet();
                    List<string> typeNameList = null;
                    List<string> paramNameList = null;
                    foreach (DataTable table in result.Tables)
                    {
                        try
                        {
                            int dataRowIndex = 0;
                            Dictionary<string, object> param = new Dictionary<string, object>()
                            {
                                { "creaetClassName", table.TableName }
                            };

                            JArray dataRowList = new JArray();

                            foreach (DataRow row in table.Rows)
                            {
                                if (dataRowIndex < 3)
                                {
                                    switch (dataRowIndex)
                                    {
                                        case 0:
                                            param.Add("fieldCount", row.ItemArray.Length);
                                            paramNameList = row.ItemArray.ToList().ConvertAll<string>(x => x.ToString());
                                            param.Add("fieldNames", paramNameList);
                                            break;
                                        case 1: break;
                                        case 2:
                                            typeNameList = row.ItemArray.ToList().ConvertAll<string>(x => x.ToString());
                                            param.Add("typeNames", typeNameList);
                                            break;
                                    }
                                    ++dataRowIndex;
                                    continue;
                                }

                                ++dataRowIndex;
                                dataRowList.Add(ConvertValueToJson(paramNameList, typeNameList, row.ItemArray));
                            }

                            var text1 = new ExcelConvert.SRC.T4Script.GenTable();
                            text1.Session = param;
                            text1.Initialize();

                            WriteFile($@"{m_outCsPath}\table_{table.TableName}.cs", text1.TransformText());
                            WriteFile($@"{m_outJsonPath}\table_{table.TableName}.json", dataRowList.ToString());
                        }
                        catch (Exception ex)
                        {
                            m_lcoaCallback?.Invoke($"{table.TableName} Exception. Reason : {ex.Message}");
                        }
                    }
                }
            }
        }

        protected JObject ConvertValueToJson(List<string> paramNameList, List<string> typeNameList, object[] value)
        {
            int count = typeNameList.Count;
            JObject jsonData = new JObject();

            for (int i = 0; i < count; ++i)
            {
                var name = typeNameList[i];

                if (string.Compare(name, "ignore", true) == 0)
                {
                    continue;
                }

                if (string.Compare(name, "byte", true) == 0)
                {
                    var castValue = value[i] as byte?;
                    jsonData.Add(paramNameList[i], castValue.GetValueOrDefault());
                }
                else if (string.Compare(name,"int", true) == 0)
                {
                    int.TryParse(value[i].ToString(), out int result);
                    jsonData.Add(paramNameList[i], result);
                }
                else if (string.Compare(name, "uint", true) == 0)
                {
                    uint.TryParse(value[i].ToString(), out uint result);
                    jsonData.Add(paramNameList[i], result);
                }
                else if (string.Compare(name, "short", true) == 0)
                {
                    short.TryParse(value[i].ToString(), out short result);
                    jsonData.Add(paramNameList[i], result);
                }
                else if (string.Compare(name, "ushort", true) == 0)
                {
                    ushort.TryParse(value[i].ToString(), out ushort result);
                    jsonData.Add(paramNameList[i], result);
                }
                else if (string.Compare(name, "long", true) == 0)
                {
                    long.TryParse(value[i].ToString(), out long result);
                    jsonData.Add(paramNameList[i], result);
                }
                else if (string.Compare(name, "ulong", true) == 0)
                {
                    ulong.TryParse(value[i].ToString(), out ulong result);
                    jsonData.Add(paramNameList[i], result);
                }
                else if (string.Compare(name, "float", true) == 0)
                {
                    float.TryParse(value[i].ToString(), out float result);
                    jsonData.Add(paramNameList[i], result);
                }
                else if (string.Compare(name, "double", true) == 0)
                {
                    double.TryParse(value[i].ToString(), out double result);
                    jsonData.Add(paramNameList[i], result);
                }
                else if (string.Compare(name, "string", true) == 0 || name.Contains("enum") || name.Contains("ENUM"))
                {
                    jsonData.Add(paramNameList[i], value[i] as string);
                }
                else if (string.Compare(name, "datetime", true) == 0)
                {
                    // DateTime 파싱할때 Utc 타임 처리 합니다.
                    if (DateTime.TryParse(value[i] as string, out DateTime time))
                    {
                        var changeUtcTime = DateTime.SpecifyKind(time, DateTimeKind.Utc);
                        jsonData.Add(paramNameList[i], changeUtcTime);
                    }
                }
            }
            return jsonData;
        }

        protected void WriteFile(string path, string content)
        {
            using (var fileWriter = new StreamWriter(path))
            {
                fileWriter.Write(content);
                m_lcoaCallback?.Invoke($"{path} Create");
            }
        }
    }
}
