using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.IO;

namespace projectBaseline
{
    public class DataFileSchema
    {
        public class ColumnInfo
        {
            public string Name { get; set; }
            public Type ClrType { get; set; }
            public string SqlType { get; set; }
            public TypeConverter TypeConvertor { get; set; }
            static public int sc_defaultIntValue = -1;
            static public double sc_defaultDoubleValue = -1;
            
            public void SetType(Type type)
            {
                ClrType = type;
            }
        }

        public List<ColumnInfo> ColumnsInfoList { get; private set; }
        public int Length { get { return ColumnsInfoList.Count; } }

        // These are another way to access the ColumnInfo fields as an individual separate list.
        public List<string> ColumnsNameList { get; private set; }
        public List<Type> ColumnsClrTypeList { get; private set; }

        private DataFileSchema()
        {
            ColumnsInfoList = new List<ColumnInfo>();
            ColumnsNameList = new List<string>();
            ColumnsClrTypeList = new List<Type>();
        }

        public DataFileSchema(string schemaSrting)
            : this()
        {
            // Expected format for the input schema string:
            // Col0Name:Col0Type,Col1Name:Col1Type,....
            // If ColxType is not present, it's set to default base on ColxName.
            // E.g.:
            // SiteUrl,SiteSize:int,SpamScore:int

            string[] columnsInfoStringList = schemaSrting.Split(',');

            foreach (string columnInfoString in columnsInfoStringList)
            {
                string[] columnNameAndType = columnInfoString.Split(':');

                if (columnNameAndType.Length < 1 || columnNameAndType.Length > 2)
                {
                    throw new ArgumentException("Invalid schema format!");
                }

                ColumnInfo columnInfo = new ColumnInfo();
                columnInfo.Name = columnNameAndType[0];

                // Set default values
                Type columnType = typeof(string);

                // Overwrite default values if type is defined in the input string
                if (columnNameAndType.Length == 2)
                {
                    switch (columnNameAndType[1].ToLowerInvariant())
                    {
                    case "string":
                        columnType = typeof(string);
                        break;

                    case "int":
                        columnType = typeof(int);
                        break;

                    case "double":
                        columnType = typeof(double);
                        break;

                    case "datetime":
                        columnType = typeof(DateTime);
                        break;

                    default:
                        throw new ArgumentException("Invalid type in schema");
                    }
                }

                columnInfo.SetType(columnType);
                columnInfo.TypeConvertor = TypeDescriptor.GetConverter(columnInfo.ClrType);

                // Why do we need both Clr and Sql types? Why not just use one?
                // Create type converters for mapping input string into column type
                //TypeDescriptor t = TypeDescriptor.GetConverter(System.Data.SqlTypes.SqlInt32);

                ColumnsInfoList.Add(columnInfo);
                ColumnsNameList.Add(columnInfo.Name);
                ColumnsClrTypeList.Add(columnInfo.ClrType);
            }
        }

        public DataFileSchema(DataTable sampleRow)
            : this()
        {
            foreach (DataColumn dataColumn in sampleRow.Columns)
            {
                ColumnInfo columnInfo = new ColumnInfo();
                columnInfo.Name = dataColumn.ColumnName;
                columnInfo.ClrType = dataColumn.DataType;

                // we don't need SqlType in this case
                columnInfo.SetType(columnInfo.ClrType);
                columnInfo.TypeConvertor = TypeDescriptor.GetConverter(columnInfo.ClrType);

                ColumnsInfoList.Add(columnInfo);
                ColumnsNameList.Add(columnInfo.Name);
                ColumnsClrTypeList.Add(columnInfo.ClrType);
            }
        }
    }

    public class DataFileSettings
    {
        public string DataFilePath { get; set; }
        public char DataDelimiter { get; set; }
        public char DataEscapeChar { get; set; }
        public bool DataHasHeader { get; set; }
        public string DataSchema { get; set; }
    }

    public class DataFileReader
    {
        public DataFileSchema Schema { get; private set; }
        public DataTable Data { get; private set; }
        public DataFileSettings Settings { get; private set; }

        private const string c_delimiterReplacement = "&^%$#";

        public DataFileReader(DataFileSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentException("Invalid settings");
            }
            Settings = settings;

            Schema = new DataFileSchema(Settings.DataSchema);

            if (Schema.Length < 1)
            {
                throw new ArgumentException("Invalid number of columns in the schema");
            }

            Data = new DataTable();

            foreach (var columnInfo in Schema.ColumnsInfoList)
            {
                Data.Columns.Add(columnInfo.Name, columnInfo.ClrType);
            }

            var filePath = Settings.DataFilePath;
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("File {0} doesn't exist!", filePath));
            }

            using (StreamReader streamFile = new StreamReader(filePath))
            {
                int lineNumber = 0;
                while (!streamFile.EndOfStream)
                {
                    string dataLine = streamFile.ReadLine();

                    if (++lineNumber == 1 && Settings.DataHasHeader)
                    {
                        continue;
                    }

                    dataLine = Escape(dataLine);
                    string[] columns = dataLine.Split(Settings.DataDelimiter);

                    if (columns.Length != Data.Columns.Count)
                    {
                        throw new Exception(string.Format("{0} format error!", filePath));
                    }

                    DataRow row = Data.NewRow();

                    for (int col = 0; col < Data.Columns.Count; col++)
                    {
                        string value = Unescape(columns[col]);

                        if (string.IsNullOrEmpty(value))
                        {
                            if (Schema.ColumnsInfoList[col].ClrType == typeof(int))
                            {
                                row[col] = DataFileSchema.ColumnInfo.sc_defaultIntValue;
                            }
                            else if (Schema.ColumnsInfoList[col].ClrType == typeof(double))
                            {
                                row[col] = DataFileSchema.ColumnInfo.sc_defaultDoubleValue;
                            }
                            else if (Schema.ColumnsInfoList[col].ClrType == typeof(DateTime))
                            {
                                row[col] = DateTime.MinValue;
                            }
                            else
                            {
                                row[col] = DBNull.Value;
                            }
                        }
                        else
                        {
                            row[col] = Schema.ColumnsInfoList[col].TypeConvertor.ConvertFrom(value);
                        }
                    }
                    Data.Rows.Add(row);
                }
            }
        }

        private string Escape(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return null;
            }

            if (line.Count(x => x == Settings.DataEscapeChar) % 2 != 0)
            {
                // unbalance escape chars
                return line;
            }

            var tokens = line.Split(Settings.DataEscapeChar);
            tokens = tokens.Select((t, i) => i % 2 == 0 ? t : t.Replace(Settings.DataDelimiter.ToString(), c_delimiterReplacement)).ToArray();
            //tokens = tokens.Select((t, i) => DummyFunc(t,i)).ToArray();

            return string.Join(Settings.DataEscapeChar.ToString(), tokens);
        }

        //private string DummyFunc(string t, int i)
        //{
        //    t = i % 2 == 0 ? t : t.Replace(Settings.DataEscapeChar.ToString(), c_escapeCharReplacement);
        //    return t;
        //}

        private string Unescape(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return null;
            }

            return line.Replace(c_delimiterReplacement, Settings.DataDelimiter.ToString());
        }

    }
}

