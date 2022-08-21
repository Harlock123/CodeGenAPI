using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CodeGenAPI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using CodeGenAPI.Attributes;
    using CodeGenAPI.Models;
    using CodeGenAPI;
    //using Bogus.Extensions.UnitedStates;
    using System.Data.SqlClient;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System.Text;
    using System.Configuration;
    using System.Data;
    using Newtonsoft.Json.Linq;

    [Route("api/[controller]")]
    [ApiController]
    //[ApiKeyAuthorize]
    [EnableCors("DefaultPolicy")]
    public class SchemaDiscoveryController : ControllerBase
    {
        string IDFIELDTYPE = "";
        string IDFIELDNAME = "";
        string TableName = "";
        bool AUTONUMBER = false;

        List<Field> TheFields = null;

        [HttpGet]
        [Route("GetAll")]
        public string GetAll(string CN = "DBwSSPI_Login")
        {
            // On big databases this will likely cause timeouts and other squirrly behaiviors for tool like the CrapTacular SWAGGER
            string result = "RESULT";

            CN = FetchActualConnectionString(CN);

            SqlConnection cn = new SqlConnection(CN);

            cn.Open();

            var sqlstring = "select A.*," +
                            "B.TABLE_TYPE," +
                            "(SELECT OBJECT_ID(A.TABLE_NAME)) as TABLEID," +
                            "(SELECT IS_IDENTITY FROM SYS.columns SC WHERE SC.object_id = (SELECT OBJECT_ID(A.TABLE_NAME)) AND SC.NAME = A.COLUMN_NAME  ) as IS_IDENTITY " +
                            "from INFORMATION_SCHEMA.COLUMNS as A " +
                            "LEFT OUTER JOIN " +
                            "INFORMATION_SCHEMA.TABLES B on A.TABLE_NAME = B.TABLE_NAME " +
                            "ORDER BY A.TABLE_NAME,A.ORDINAL_POSITION";


            SqlDataAdapter da = new SqlDataAdapter(sqlstring, cn);

            System.Data.DataSet d = new System.Data.DataSet();

            da.Fill(d);

            result = JsonConvert.SerializeObject(d, Formatting.Indented);

            d.Dispose();
            da.Dispose();
            cn.Close();
            cn.Dispose();

            return result;
        }

        [HttpGet]
        [Route("GetTables")]
        public string GetTables(
            string CN = "DBwSSPI_Login")
        {
            string result = "RESULT";

            CN = FetchActualConnectionString(CN);

            SqlConnection cn = new SqlConnection(CN);


            cn.Open();

            var sqlstring = "SELECT * " +
                "FROM INFORMATION_SCHEMA.TABLES " +
                "ORDER BY TABLE_NAME";

            SqlDataAdapter da = new SqlDataAdapter(sqlstring, cn);

            System.Data.DataSet d = new System.Data.DataSet();

            da.Fill(d);

            result = JsonConvert.SerializeObject(d, Formatting.Indented);

            d.Dispose();
            da.Dispose();
            cn.Close();
            cn.Dispose();

            return result;
        }

        [HttpGet]
        [Route("GetListOfTables")]
        public IEnumerable<string> GetListOfTables(
            string CN = "DBwSSPI_Login")
        {
            List<string> result = new List<string>();

            CN = FetchActualConnectionString(CN);

            SqlConnection cn = new SqlConnection(CN);


            cn.Open();

            var sqlstring = "SELECT * " +
                "FROM INFORMATION_SCHEMA.TABLES " +
                "ORDER BY TABLE_NAME";

            SqlCommand cmd = new SqlCommand(sqlstring, cn);

            SqlDataReader r = cmd.ExecuteReader();

            while (r.Read())
            {
                result.Add(r["TABLE_NAME"].ToString());
            }
            r.Close();
            cmd.Dispose();

            cn.Close();
            cn.Dispose();

            return result.ToArray();
        }

        [HttpGet]
        [Route("GetTableSchema")]
        public string GetTableSchema(
            string CN = "DBwSSPI_Login",
            string TN = "MemberMain")
        {
            string result = "RESULT";

            CN = FetchActualConnectionString(CN);

            SqlConnection cn = new SqlConnection(CN);


            cn.Open();

            var sqlstring = "select A.*," +
                            "B.TABLE_TYPE," +
                            "(SELECT OBJECT_ID(A.TABLE_NAME)) as TABLEID," +
                            "(SELECT IS_IDENTITY FROM SYS.columns SC WHERE SC.object_id = (SELECT OBJECT_ID(A.TABLE_NAME)) AND SC.NAME = A.COLUMN_NAME  ) as IS_IDENTITY " +
                            "from INFORMATION_SCHEMA.COLUMNS as A " +
                            "LEFT OUTER JOIN " +
                            "INFORMATION_SCHEMA.TABLES B on A.TABLE_NAME = B.TABLE_NAME " +
                            "WHERE A.TABLE_NAME = @TABLENAME " +
                            "ORDER BY A.TABLE_NAME,A.ORDINAL_POSITION";


            SqlDataAdapter da = new SqlDataAdapter(sqlstring, cn);

            da.SelectCommand.Parameters.AddWithValue("@TABLENAME", TN);

            System.Data.DataSet d = new System.Data.DataSet();

            da.Fill(d);

            result = JsonConvert.SerializeObject(d, Formatting.Indented);

            d.Dispose();
            da.Dispose();
            cn.Close();
            cn.Dispose();

            return result;
        }

        [HttpGet]
        [Route("GetTableSchemaFields")]
        public IEnumerable<CodeGenAPI.Models.Field> GetTableSchemaFields(
            string CN = "DBwSSPI_Login",
            string TN = "MemberMain")
        {
            List<CodeGenAPI.Models.Field> result = new List<Field>();

            try
            {
                CN = FetchActualConnectionString(CN);

                SqlConnection cn = new SqlConnection(CN);

                cn.Open();

                var sqlstring = "select A.*," +
                                "B.TABLE_TYPE," +
                                "(SELECT OBJECT_ID(A.TABLE_NAME)) as TABLEID," +
                                "(SELECT IS_IDENTITY FROM SYS.columns SC WHERE SC.object_id = (SELECT OBJECT_ID(A.TABLE_NAME)) AND SC.NAME = A.COLUMN_NAME  ) as IS_IDENTITY " +
                                "from INFORMATION_SCHEMA.COLUMNS as A " +
                                "LEFT OUTER JOIN " +
                                "INFORMATION_SCHEMA.TABLES B on A.TABLE_NAME = B.TABLE_NAME " +
                                "WHERE A.TABLE_NAME = @TABLENAME " +
                                "ORDER BY A.TABLE_NAME,A.ORDINAL_POSITION";


                SqlCommand cmd = new SqlCommand(sqlstring, cn);

                cmd.Parameters.Add("@TABLENAME", System.Data.SqlDbType.VarChar).Value = TN;

                SqlDataReader r = cmd.ExecuteReader();

                //"TABLE_CATALOG": "OPENCUDmDB",
                //"TABLE_SCHEMA": "dbo",
                //"TABLE_NAME": "MemberMain",
                //"COLUMN_NAME": "ID",
                //"ORDINAL_POSITION": 1,
                //"COLUMN_DEFAULT": null,
                //"IS_NULLABLE": "NO",
                //"DATA_TYPE": "int",
                //"CHARACTER_MAXIMUM_LENGTH": null,
                //"CHARACTER_OCTET_LENGTH": null,
                //"NUMERIC_PRECISION": 10,
                //"NUMERIC_PRECISION_RADIX": 10,
                //"NUMERIC_SCALE": 0,
                //"DATETIME_PRECISION": null,
                //"CHARACTER_SET_CATALOG": null,
                //"CHARACTER_SET_SCHEMA": null,
                //"CHARACTER_SET_NAME": null,
                //"COLLATION_CATALOG": null,
                //"COLLATION_SCHEMA": null,
                //"COLLATION_NAME": null,
                //"DOMAIN_CATALOG": null,
                //"DOMAIN_SCHEMA": null,
                //"DOMAIN_NAME": null,
                //"TABLE_TYPE": "BASE TABLE",
                //"TABLEID": 98099390,
                //"IS_IDENTITY": true

                while (r.Read())
                {
                    Field f = new Field();

                    if (r["IS_NULLABLE"].ToString() == "NO")
                    {
                        f.AllowNulls = false;
                    }
                    else
                    {
                        f.AllowNulls = true;
                    }

                    f.FieldName = r["COLUMN_NAME"].ToString();
                    f.FieldNameConverted = f.FieldName.Replace(" ", "_");
                    f.FieldType = r["DATA_TYPE"].ToString().ToUpper();


                    f.IsIdentity = r.GetBoolean(r.GetOrdinal("IS_IDENTITY"));

                    if (r.IsDBNull(r.GetOrdinal("NUMERIC_SCALE")))
                        f.Scale = 0;
                    else
                        f.Scale = Int32.Parse(r["NUMERIC_SCALE"].ToString());


                    if (r.IsDBNull(r.GetOrdinal("NUMERIC_PRECISION")))
                        f.Precision = 0;
                    else
                        f.Precision = Int32.Parse(r["NUMERIC_PRECISION"].ToString());

                    if (r.IsDBNull(r.GetOrdinal("CHARACTER_MAXIMUM_LENGTH")))
                        f.MaxLength = 0;
                    else
                        f.MaxLength = r.GetInt32(r.GetOrdinal("CHARACTER_MAXIMUM_LENGTH"));

                    f.TABLENAME = TN;

                    result.Add(f);

                }
                r.Close();
                cmd.Dispose();
                cn.Close();
                cn.Dispose();

            }
            catch (Exception ex)
            {
                Field f = new Field();

                f.FieldName = "ERROR WITH GretTableSchemaFields";
                f.FieldNameConverted = ex.Message;

                result.Add(f);
            }


            return result;

        }

        [HttpGet]
        [Route("GetTableModel")]
        public string GetTableModel(
            string CN = "DBwSSPI_Login",
            string TN = "MemberMain")
        {
            string result = "";

            //CN = FetchActualConnectionString(CN);

            try
            {
                TheFields = (List<Field>)GetTableSchemaFields(CN, TN);
                TableName = TN;

                //string IDFIELDTYPE = "";
                //string IDFIELDNAME = "";
                //bool AUTONUMBER = false;

                foreach (Field f in TheFields)
                {
                    if (f.IsIdentity)
                    {
                        IDFIELDTYPE = f.FieldType;
                        IDFIELDNAME = f.FieldName;
                        AUTONUMBER = true;
                        break;
                    }
                }

                string s = "";

                s = "using System;\n" +
                        "using System.ComponentModel;\n" +
                        "using System.Data;\n" +
                        "using System.Data.SqlClient;\n\n";

                s += "public partial class " + TN + " : INotifyPropertyChanged\n" +
                    "{\n\n" +
                    "#region Declarations\n" +
                    "string _classDatabaseConnectionString = \"\";\n" +
                    "string _bulkinsertPath = \"\";\n\n" +
                    "SqlConnection _cn = new SqlConnection();\n" +
                    "SqlCommand _cmd = new SqlCommand();\n\n" +
                    "// Backing Variables for Properties\n";

                foreach (Field f in TheFields)
                {
                    if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                        f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                        f.FieldType == "SYSNAME")
                    {
                        s += "string _" + f.FieldNameConverted + " = \"\";\n";
                    }

                    if (f.FieldType == "INT" || f.FieldType == "SMALLINT" || f.FieldType == "TINYINT")
                    {
                        s += "int _" + f.FieldNameConverted + " = 0;\n";
                    }

                    if (f.FieldType == "BIGINT")
                    {
                        s += "long _" + f.FieldNameConverted + " = 0;\n";
                    }

                    if (f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" || f.FieldType == "FLOAT")
                    {
                        s += "double _" + f.FieldNameConverted + " = 0.0;\n";
                    }

                    if (f.FieldType == "DECIMAL")
                    {
                        s += "double _" + f.FieldNameConverted + " = 0.0;\n";
                    }

                    if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                    {
                        s += "DateTime _" + f.FieldNameConverted + " = Convert.ToDateTime(null);\n";
                    }

                    if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                    {
                        s += "bool _" + f.FieldNameConverted + " = false;\n";
                    }
                }


                s += "\n" +
                "#endregion\n\n" +
                "#region Properties\n\n" +
                "public string classDatabaseConnectionString\n" +
                "{\n";

                s += "get{return _classDatabaseConnectionString;}\n";

                s += "set{_classDatabaseConnectionString = value;}\n}\n\n";

                s += "public string bulkinsertPath\n{\n";

                s += "get{return _bulkinsertPath;} \n";

                s += "set{_bulkinsertPath = value;} \n}\n\n";


                foreach (Field f in TheFields)
                {
                    if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                        f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                        f.FieldType == "SYSNAME")
                    {

                        if (f.MaxLength < 0)
                        {
                            s += "public string " + f.FieldNameConverted + "\n{\n" +
                           "get{ return _" + f.FieldNameConverted + ";}\n" +
                           "set{ \n" +
                           "if (value != null && value.Length > " + int.MaxValue.ToString() + ")\n" +
                           "{ _" + f.FieldNameConverted + " = value.Substring(0," + int.MaxValue.ToString() + ");}\n" +
                           "else { _" + f.FieldNameConverted + " = value;\n" +
                           "RaisePropertyChanged(\"" + f.FieldNameConverted + "\");}\n}\n}\n\n";
                        }
                        else
                        {
                            s += "public string " + f.FieldNameConverted + "\n{\n" +
                           "get{ return _" + f.FieldNameConverted + ";}\n" +
                           "set{ \n" +
                           "if (value != null && value.Length > " + f.MaxLength.ToString() + ")\n" +
                           "{ _" + f.FieldNameConverted + " = value.Substring(0," + f.MaxLength.ToString() + ");}\n" +
                           "else { _" + f.FieldNameConverted + " = value;\n" +
                           "RaisePropertyChanged(\"" + f.FieldNameConverted + "\");}\n}\n}\n\n";
                        }

                    }

                    if (f.FieldType == "INT" || f.FieldType == "SMALLINT" || f.FieldType == "TINYINT")
                    {
                        s += "public int " + f.FieldNameConverted + "\n{\n" +
                                "get{ return _" + f.FieldNameConverted + ";}\n" +
                                "set{ _" + f.FieldNameConverted + " = value;\n" +
                                "RaisePropertyChanged(\"" + f.FieldNameConverted + "\");}\n}\n\n";

                    }

                    if (f.FieldType == "BIGINT")
                    {
                        s += "public long " + f.FieldNameConverted + "\n{\n" +
                                "get{ return _" + f.FieldNameConverted + ";}\n" +
                                "set{ _" + f.FieldNameConverted + " = value;\n" +
                                "RaisePropertyChanged(\"" + f.FieldNameConverted + "\");}\n}\n\n";

                    }

                    if (f.FieldType == "DECIMAL" || f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" || f.FieldType == "FLOAT")
                    {
                        s += "public double " + f.FieldNameConverted + "\n{\n" +
                                "get{ return _" + f.FieldNameConverted + ";}\n" +
                                "set{ _" + f.FieldNameConverted + " = value;\n" +
                                "RaisePropertyChanged(\"" + f.FieldNameConverted + "\");}\n}\n\n";

                    }

                    if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                    {

                        s += "public DateTime " + f.FieldNameConverted + "\n{\n" +
                                 "get{ return _" + f.FieldNameConverted + ";}\n" +
                                 "set{ _" + f.FieldNameConverted + " = value;\n" +
                                "RaisePropertyChanged(\"" + f.FieldNameConverted + "\");}\n}\n\n";

                    }

                    if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                    {
                        s += "public bool " + f.FieldNameConverted + "\n{\n" +
                                "get{ return _" + f.FieldNameConverted + ";}\n" +
                                "set{ _" + f.FieldNameConverted + " = value;\n" +
                                "RaisePropertyChanged(\"" + f.FieldNameConverted + "\");}\n}\n\n";

                    }
                }

                s += "\n" +
                    "#endregion\n\n" +
                    "#region Implement INotifyPropertyChanged \n\n" +
                    "public event PropertyChangedEventHandler PropertyChanged;\n" +
                    "public void RaisePropertyChanged(string propertyName)\n" +
                    "{\n" +
                    "PropertyChangedEventHandler handler = PropertyChanged;\n" +
                    "if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));\n" +
                    "}\n" +
                    "#endregion\n\n";

                s += "#region Constructor\n\n" +
                    "public " + TN + "()\n" +
                    "{\n" +
                    "// Constructor code goes here.\n" +
                    "Initialize();\n" +
                    "}\n\n" +
                    "public " + TN + "(string DSN)\n" +
                    "{\n" +
                    "// Constructor code goes here.\n" +
                    "Initialize();\n" +
                    "classDatabaseConnectionString = DSN;\n" +
                    "}\n\n" +
                    "#endregion\n\n";

                s += "public void CopyFields(SqlDataReader r)\n";
                s += "{\n";
                s += "try\n";
                s += "{\n";

                foreach (Field f in TheFields)
                {
                    if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                        f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                        f.FieldType == "SYSNAME")
                    {
                        s += "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                        s += "{\n";
                        s += "_" + f.FieldNameConverted + " = r[\"" + f.FieldName + "\"] + \"\";\n";
                        s += "}\n";
                    }

                    if (f.FieldType == "INT" || f.FieldType == "SMALLINT" || f.FieldType == "TINYINT")
                    {
                        s += "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                        s += "{\n";
                        s += "_" + f.FieldNameConverted + " = Convert.ToInt32(r[\"" + f.FieldName + "\"]);\n";
                        s += "}\n";
                    }

                    if (f.FieldType == "BIGINT")
                    {
                        s += "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                        s += "{\n";
                        s += "_" + f.FieldNameConverted + " = Convert.ToInt64(r[\"" + f.FieldName + "\"]);\n";
                        s += "}\n";
                    }

                    if (f.FieldType == "DECIMAL" || f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" || f.FieldType == "FLOAT")
                    {
                        s += "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                        s += "{\n";
                        s += "_" + f.FieldNameConverted + " = Convert.ToDouble(r[\"" + f.FieldName + "\"]);\n";
                        s += "}\n";
                    }

                    if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                    {
                        s += "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                        s += "{\n";
                        s += "_" + f.FieldNameConverted + " = Convert.ToDateTime(r[\"" + f.FieldName + "\"]);\n";
                        s += "}\n";
                    }

                    if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                    {
                        s += "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                        s += "{\n";
                        s += "_" + f.FieldNameConverted + " = Convert.ToBoolean(r[\"" + f.FieldName + "\"]);\n";
                        s += "}\n";
                    }
                }

                s += "}\n";
                s += "catch (Exception ex)\n";
                s += "{\n";
                s += "throw(new Exception(\"" + TN + ".CopyFields \" +  ex.ToString()));\n";
                s += "}\n";
                s += "}\n\n";

                s += "public void Initialize()\n";
                s += "{\n";

                foreach (Field f in TheFields)
                {
                    if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                        f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                        f.FieldType == "SYSNAME")
                    {
                        s += "_" + f.FieldNameConverted + " = \"\";\n";
                    }

                    if (f.FieldType == "INT" || f.FieldType == "SMALLINT" || f.FieldType == "TINYINT")
                    {
                        s += "_" + f.FieldNameConverted + " = 0;\n";
                    }

                    if (f.FieldType == "BIGINT")
                    {
                        s += "_" + f.FieldNameConverted + " = 0;\n";
                    }

                    if (f.FieldType == "DECIMAL" || f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" || f.FieldType == "FLOAT")
                    {
                        s += "_" + f.FieldNameConverted + " = 0.0;\n";
                    }

                    if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                    {
                        s += "_" + f.FieldNameConverted + " = Convert.ToDateTime(null);\n";
                    }

                    if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                    {
                        s += "_" + f.FieldNameConverted + " = false;\n";
                    }
                }

                s += "}\n\n";

                if (IDFIELDTYPE == "BIGINT" || IDFIELDTYPE == "LONG")
                {
                    s += "public void Read(System.Int64 idx)\n";
                }
                else
                {
                    if (IDFIELDTYPE == "INT" || IDFIELDTYPE == "SMALLINT" || IDFIELDTYPE == "TINYINT")
                    {
                        s += "public void Read(System.Int32 idx)\n";
                    }
                    else
                    {
                        // this should never happen
                        if (IDFIELDTYPE == "DECIMAL" || IDFIELDTYPE == "DOUBLE" || IDFIELDTYPE == "MONEY" || IDFIELDTYPE == "CURRENCY" || IDFIELDTYPE == "FLOAT")
                        {
                            s += "public void Read(System.Double idx)\n";
                        }
                        else
                        {
                            // default to a long
                            s += "public void Read(string idx)\n";
                        }
                    }
                }

                s += "{\n";
                s += "try\n";
                s += "{\n";

                s += "string sql =\"Select * from " + TN + " WHERE " + IDFIELDNAME + " = @ID\";\n";
                s += "SqlConnection cn = new SqlConnection(_classDatabaseConnectionString);\n";
                s += "cn.Open();\n";
                s += "SqlCommand cmd = new SqlCommand(sql,cn);\n";

                if (IDFIELDTYPE == "BIGINT" || IDFIELDTYPE == "LONG")
                {
                    s += "cmd.Parameters.Add(\"@ID\",System.Data.SqlDbType.BigInt).Value = idx;\n";
                }
                else
                {
                    if (IDFIELDTYPE == "INT" || IDFIELDTYPE == "SMALLINT" || IDFIELDTYPE == "TINYINT")
                    {
                        s += "cmd.Parameters.Add(\"@ID\",System.Data.SqlDbType.Int).Value = idx;\n";
                    }
                    else
                    {
                        // this should never happen
                        if (IDFIELDTYPE == "DOUBLE" || IDFIELDTYPE == "MONEY" || IDFIELDTYPE == "CURRENCY" || IDFIELDTYPE == "FLOAT")
                        {
                            s += "cmd.Parameters.Add(\"@ID\",System.Data.SqlDbType.Money).Value = idx;\n";
                        }
                        else
                        {
                            // default to a long
                            s += "cmd.Parameters.Add(\"@ID\",System.Data.SqlDbType.VarChar).Value = idx;\n";
                        }
                    }
                }

                s += "SqlDataReader r = cmd.ExecuteReader();\n";
                s += "while (r.Read())\n";
                s += "{\n";
                s += "this.CopyFields(r);\n";
                s += "}\n";
                s += "r.Close();\n";
                s += "cmd.Cancel();\n";
                s += "cmd.Dispose();\n";
                s += "cn.Close();\n";
                s += "cn.Dispose();\n";
                s += "}\n";
                s += "catch (Exception ex)\n";
                s += "{\n";
                s += "throw(new Exception(\"" + TN + ".Read \" +  ex.ToString()));\n";
                s += "}\n";
                s += "}\n\n";

                s += "public void Update()\n";

                s += "{\n";
                s += "try\n";
                s += "{\n";

                if (AUTONUMBER)
                    s += "string sql = GetParameterSQL();\n";
                else
                    s += "string sql = GetParameterSQLForUpdate();\n";

                s += "SqlConnection cn = new SqlConnection(_classDatabaseConnectionString);\n";
                s += "cn.Open();\n";
                s += "SqlCommand cmd = new SqlCommand(sql,cn);\n";

                foreach (Field f in TheFields)
                {
                    if (f.FieldName != IDFIELDNAME || (!AUTONUMBER && f.FieldName == IDFIELDNAME))
                    {

                        if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                            f.FieldType == "TEXT" || f.FieldType == "SYSNAME")
                        {
                            if (f.AllowNulls) // also add the UI check here
                            {
                                s += "if (this._" + f.FieldNameConverted + " == null || this._" + f.FieldNameConverted + " == \"\" || this._" + f.FieldNameConverted + " == string.Empty)\n" +
                                     "{\n " +
                                     "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.VarChar).Value = DBNull.Value;\n" +
                                     "}\n" +
                                     "else\n" +
                                     "{\n " +
                                     "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.VarChar).Value = this._" + f.FieldNameConverted + ";\n" +
                                     "}\n";
                            }
                            else
                            {
                                s += "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.VarChar).Value = this._" + f.FieldNameConverted + ";\n";
                            }
                        }

                        if (f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID")
                        {
                            s += "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.UniqueIdentifier).Value = System.Guid.Parse(this._" + f.FieldNameConverted + ");\n";
                        }

                        if (f.FieldType == "INT")
                        {
                            s += "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.Int).Value = this._" + f.FieldNameConverted + ";\n";
                        }

                        if (f.FieldType == "SMALLINT")
                        {
                            s += "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.SmallInt).Value = this._" + f.FieldNameConverted + ";\n";
                        }

                        if (f.FieldType == "TINYINT")
                        {
                            s += "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.TinyInt).Value = this._" + f.FieldNameConverted + ";\n";
                        }

                        if (f.FieldType == "BIGINT")
                        {
                            s += "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.BigInt).Value = this._" + f.FieldNameConverted + ";\n";
                        }

                        if (f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" || f.FieldType == "FLOAT")
                        {
                            s += "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.Money).Value = this._" + f.FieldNameConverted + ";\n";
                        }

                        if (f.FieldType == "DECIMAL")
                        {
                            s += "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.Decimal).Value = this._" + f.FieldNameConverted + ";\n";
                        }

                        if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                        {
                            s += "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.DateTime).Value = getDateOrNull(this._" + f.FieldNameConverted + ");\n";
                        }

                        if (f.FieldType == "BOOL")
                        {
                            s += "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.Bool).Value = this._" + f.FieldNameConverted + ";\n";
                        }

                        if (f.FieldType == "BIT")
                        {
                            s += "cmd.Parameters.Add(\"@" + f.FieldNameConverted + "\",System.Data.SqlDbType.Bit).Value = this._" + f.FieldNameConverted + ";\n";
                        }


                        //System.Guid.Parse()
                    }
                }

                s += "cmd.ExecuteNonQuery();\n";
                s += "cmd.Cancel();\n";
                s += "cmd.Dispose();\n";

                if (AUTONUMBER)
                {

                    s += "if(" + IDFIELDNAME + " < 1)\n";
                    s += "{\n";
                    s += "SqlCommand cmd2 = new SqlCommand(\"SELECT @@IDENTITY\",cn);\n";

                    if (IDFIELDTYPE == "BIGINT" || IDFIELDTYPE == "LONG")
                    {
                        s += "System.Int64 ii = Convert.ToInt64(cmd2.ExecuteScalar());\n";
                    }
                    else
                    {
                        if (IDFIELDTYPE == "INT" || IDFIELDTYPE == "SMALLINT" || IDFIELDTYPE == "TINYINT")
                        {
                            s += "System.Int32 ii = Convert.ToInt32(cmd2.ExecuteScalar());\n";
                        }
                        else
                        {
                            // this should never happen
                            if (IDFIELDTYPE == "DOUBLE" || IDFIELDTYPE == "MONEY" || IDFIELDTYPE == "CURRENCY" || IDFIELDTYPE == "FLOAT")
                            {
                                s += "System.Double ii = Convert.ToDouble(cmd2.ExecuteScalar());\n";
                            }
                            else
                            {
                                // default to a long
                                s += "System.Int64 ii = Convert.ToInt64(cmd2.ExecuteScalar());\n";
                            }
                        }
                    }
                    s += "cmd2.Cancel();\n";
                    s += "cmd2.Dispose();\n";
                    s += "_" + IDFIELDNAME + " = ii;\n";
                    s += "}\n";

                }

                s += "cn.Close();\n";
                s += "cn.Dispose();\n";
                s += "}\n";
                s += "catch (Exception ex)\n";
                s += "{\n";
                s += "throw(new Exception(\"" + TN + ".Update \" +  ex.ToString()));\n";
                s += "}\n";
                s += "}\n\n";

                s += GeneratePrivateMethods();

                s += "}\n";



                result = s;

            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return DoTheIndentation(result);
        }

        [HttpGet]
        [Route("GetTableColumns")]
        public IEnumerable<String> GetTableColumns(
            string CN = "DBwSSPI_Login",
            string TN = "MemberMain")
        {
            List<String> result = new List<String>();

            CN = FetchActualConnectionString(CN);

            SqlConnection cn = new SqlConnection(CN);

            cn.Open();

            var sqlstring = "Select A.COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS as A " +
                            "Where A.TABLE_NAME = @TABLENAME Order By A.ORDINAL_POSITION";


            SqlCommand cmd = new SqlCommand(sqlstring, cn);
            cmd.Parameters.AddWithValue("@TABLENAME", TN);

            SqlDataReader r = cmd.ExecuteReader();

            while (r.Read())
            {
                result.Add(r[0].ToString());
            }
            r.Close();
            cmd.Dispose();
            cn.Close();
            cn.Dispose();

            return result;
        }
        
        [HttpGet]
        [Route("GetConfiguration")]
        public string GetConfiguration ()
        {
            string result = "";
            var _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var things = _config.GetSection("Settings:Databases").GetChildren();

            foreach(var i in things)
            {
                result += i.Key + " : " + i.Value + "\n";
            }

            return result.ToString();


        }

        [HttpGet]
        [Route("GetSchemaFieldsFromSQLCode")]
        public IEnumerable<CodeGenAPI.Models.Field> GetSchemaFieldsFromSQLCode (
            string CN = "DBwSSPI_Login", string SQLCode = "Select top 1 * from SOMETABLE")
        {
            List<CodeGenAPI.Models.Field> result = new List<CodeGenAPI.Models.Field>();

            CN = FetchActualConnectionString(CN);

            SqlConnection cn = new SqlConnection(CN);

            SqlCommand cmd = new SqlCommand(SQLCode, cn);

            //cmd.Parameters.Add("@ID",SqlDbType.)

            SqlDataAdapter ad = new SqlDataAdapter(cmd);

            DataSet ds = new DataSet();
            ad.FillSchema(ds, SchemaType.Mapped);

            var metadata = ds.Tables[0];

            foreach (DataColumn col in metadata.Columns)
            {
                Field f = new Field();

                f.AllowNulls = col.AllowDBNull;
                f.FieldName = col.ColumnName;
                f.FieldType = col.DataType.ToString();
                f.IsIdentity = col.AutoIncrement;
                f.MaxLength = col.MaxLength;
                f.Precision = 0;
                
                result.Add(f); 


            }

            ds.Dispose();
            ad.Dispose();
            cmd.Dispose();
            cn.Close();
            cn.Dispose();

            return result;
        }

        [HttpGet]
        [Route("GetInterfaceClassFromSQLCode")]
        public string GetInterfaceClassFromSQLCode(
            string CN = "DBwSSPI_Login", string SQLCode = "Select top 1 * from SOMETABLE", string ClassName = "MyAwesomeObject")
        {
            string result = "public class " + ClassName + "\n" + "{\n";

            string TheTabs = "\t";
            
            List<CodeGenAPI.Models.Field> TheFields = 
                (List<CodeGenAPI.Models.Field>)GetSchemaFieldsFromSQLCode(CN, SQLCode);

            foreach(CodeGenAPI.Models.Field theField in TheFields)
            {
                
                if (char.IsLetter(theField.FieldName.FirstOrDefault()))
                    result += TheTabs + "public " + theField.FieldType + " " + theField.FieldName + " { get; set; }\n";
                else
                    result += TheTabs + "public " + theField.FieldType + " _" + theField.FieldName + " { get; set; }\n";
            }

            result += "}\n";
            

            return result;
        }

        [HttpGet]
        [Route("GetInterfaceClassFromSpecificTableName")]
        public string GetInterfaceClassFromSpecificTableName(
            string CN = "DBwSSPI_Login", string Tname = "TNAME", string ClassName = "MyAwesomeObject")
        {
            string result = "public class " + ClassName + "\n" + "{\n";

            string TheTabs = "\t";

            string SQLCode = "Select top 1 * from " + Tname;

            List<CodeGenAPI.Models.Field> TheFields =
                (List<CodeGenAPI.Models.Field>)GetSchemaFieldsFromSQLCode(CN, SQLCode);

            foreach (CodeGenAPI.Models.Field theField in TheFields)
            {

                if (char.IsLetter(theField.FieldName.FirstOrDefault()))
                    result += TheTabs + "public " + theField.FieldType + " " + theField.FieldName + " { get; set; }\n";
                else
                    result += TheTabs + "public " + theField.FieldType + " _" + theField.FieldName + " { get; set; }\n";
            }

            result += "}\n";


            return result;
        }

        [HttpGet]
        [Route("GetAllTablesInterfaceClassesFromDataBase")]
        public string GetAllTablesInterfaceClassesFromDataBase(
            string CN = "DBwSSPI_Login")
        {
            StringBuilder result = new StringBuilder();

            IEnumerable<String> TheTables = GetListOfTables(CN);

            foreach(string TableName in TheTables)
            {
                result.Append(GetInterfaceClassFromSQLCode(CN, "SELECT TOP 1 * from [" + TableName + "]", "cls" + TableName));
                result.Append(System.Environment.NewLine);
            }


            return result.ToString();
        }

        [HttpGet]
        [Route("GetGetterWebMethodFromSQLCode")]
        public string GetGetterWebMethodFromSQLCode(
            string CN = "DBwSSPI_Login", string SQLCode = "Select top 1 * from SOMETABLE", 
            string ClassName = "MyAwesomeObject", string FilterFieldName = "SomeFieldName", Boolean ReturnSingleton = false)
           
        {
            string result = GetInterfaceClassFromSQLCode(CN, SQLCode, ClassName);
                                    
            List<CodeGenAPI.Models.Field> TheFields =
                (List<CodeGenAPI.Models.Field>)GetSchemaFieldsFromSQLCode(CN, SQLCode);

            var filterfieldtype = "string";
            var sqlfiltertype = "SqlDbType.VarChar";

            foreach (Field f in TheFields)
            {
                if (FilterFieldName.ToLower().Trim() == f.FieldName.ToLower()) 
                {
                    if (f.FieldType.ToLower().EndsWith("int32"))
                    {
                        filterfieldtype = "Int32";
                        sqlfiltertype = "SqlDbType.Int";
                    }

                    if (f.FieldType.ToLower().EndsWith("decimal"))
                    {
                        filterfieldtype = "Double";
                        sqlfiltertype = "SqlDbType.Float";
                    }

                    if (f.FieldType.ToLower().EndsWith("boolean"))
                    {
                        filterfieldtype = "Boolean";
                        sqlfiltertype = "SqlDbType.Bit";
                    }

                    if (f.FieldType.ToLower().EndsWith("datetime"))
                    {
                        filterfieldtype = "DateTime"; 
                        sqlfiltertype = "SqlDbType.DateTime";
                    }
                }
            }

            //foreach (CodeGenAPI.Models.Field theField in TheFields)
            //{
            //    result += TheTabs + theField.FieldType + " " + theField.FieldName + " { get; set; }\n";

            //}

            result += "\n\n\n";

            if (ReturnSingleton)
            {
                result += "[HttpGet]\n";
                result += "[Route(\"Get" + ClassName + "\")]\n";
                result += "public " + ClassName + " Get" + ClassName + " (" + filterfieldtype + " filt " + ")\n";
                result += "{\n";
                result += Tabify(1) + ClassName + " result = new " + ClassName + "();\n";
                result += Tabify(1) + "using (SqlConnection cn = new SqlConnection(\"" + FetchActualConnectionString(CN) + "\"))\n";
                result += Tabify(1) + "{\n";

                result += Tabify(2) + "try\n";
                result += Tabify(2) + "{\n";

                result += Tabify(3) + "cn.Open();\n";
                result += Tabify(3) + "var Sql = " + Stringify(SQLCode, 3);
                result += Tabify(3) + "Sql += \" where " + FilterFieldName + " = @filt\";\n";
                result += Tabify(3) + "using (SqlCommand cmd = new SqlCommand(Sql,cn))\n";
                result += Tabify(3) + "{\n";

                result += Tabify(4) + "cmd.Parameters.Add( \"@filt\"," + sqlfiltertype + ").Value = filt;\n";

                result += Tabify(4) + "cmd.CommandTimeout = 500;\n";
                result += Tabify(4) + "SqlDataReader r = cmd.ExecuteReader();\n";
                result += Tabify(4) + "while (r.Read())\n";
                result += Tabify(4) + "{\n";

                result += Tabify(5) + ClassName + " c = new " + ClassName + "();\n";

                foreach (Field f in TheFields)
                {
                    if (f.FieldType.ToLower().EndsWith("string"))
                        result += Tabify(5) + "c." + f.FieldName + " = r[\"" + f.FieldName + "\"] + \"\";\n";

                    if (f.FieldType.ToLower().EndsWith("boolean"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToBoolean(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToBoolean(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }

                    if (f.FieldType.ToLower().EndsWith("int32"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToInt32(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToInt32(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }

                    if (f.FieldType.ToLower().EndsWith("datetime"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToDateTime(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToDateTime(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }

                    if (f.FieldType.ToLower().EndsWith("decimal"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToDecimal(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToDecimal(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }


                }

                result += Tabify(5) + "result = c;\n";

                result += Tabify(4) + "} // End of While() \n";

                result += Tabify(3) + "cmd.Dispose();\n";

                result += Tabify(3) + "} // End of Using (SqlCommand \n";

                result += Tabify(2) + "} // End of Try\n";

                result += Tabify(2) + "catch (Exception ex)\n";
                result += Tabify(2) + "{\n";
                result += Tabify(3) + "Console.WriteLine(ex.ToString());\n";
                result += Tabify(2) + "}\n";

                result += Tabify(2) + "cn.Close();\n";

                result += Tabify(1) + "} // End of Using (SqlConnection \n";

                result += Tabify(1) + "return result;\n";

                result += "} // End of GETTER\n";
            }
            else
            {
                result += "[HttpGet]\n";
                result += "[Route(\"GetListOf" + ClassName + "\")]\n";
                result += "public List<" + ClassName + "> GetListOf" + ClassName + " (" + filterfieldtype + " filt " + ")\n";
                result += "{\n";
                result += Tabify(1) + "List<" + ClassName + "> result = new List<" + ClassName + ">();\n";
                result += Tabify(1) + "using (SqlConnection cn = new SqlConnection(\"" + FetchActualConnectionString(CN) + "\"))\n";
                result += Tabify(1) + "{\n";

                result += Tabify(2) + "try\n";
                result += Tabify(2) + "{\n";

                result += Tabify(3) + "cn.Open();\n";
                result += Tabify(3) + "var Sql = " + Stringify(SQLCode, 3);
                result += Tabify(3) + "Sql += \" where " + FilterFieldName + " = @filt\";\n";
                result += Tabify(3) + "using (SqlCommand cmd = new SqlCommand(Sql,cn))\n";
                result += Tabify(3) + "{\n";

                result += Tabify(4) + "cmd.Parameters.Add( \"@filt\"," + sqlfiltertype + ").Value = filt;\n";

                result += Tabify(4) + "cmd.CommandTimeout = 500;\n";
                result += Tabify(4) + "SqlDataReader r = cmd.ExecuteReader();\n";
                result += Tabify(4) + "while (r.Read())\n";
                result += Tabify(4) + "{\n";

                result += Tabify(5) + ClassName + " c = new " + ClassName + "();\n";

                foreach (Field f in TheFields)
                {
                    if (f.FieldType.ToLower().EndsWith("string"))
                        result += Tabify(5) + "c." + f.FieldName + " = r[\"" + f.FieldName + "\"] + \"\";\n";

                    if (f.FieldType.ToLower().EndsWith("boolean"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToBoolean(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToBoolean(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }

                    if (f.FieldType.ToLower().EndsWith("int32"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToInt32(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToInt32(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }

                    if (f.FieldType.ToLower().EndsWith("datetime"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToDateTime(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToDateTime(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }

                    if (f.FieldType.ToLower().EndsWith("decimal"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToDecimal(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToDecimal(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }


                }

                result += Tabify(5) + "result.Add(c);\n";

                result += Tabify(4) + "} // End of While() \n";

                result += Tabify(3) + "cmd.Dispose();\n";

                result += Tabify(3) + "} // End of Using (SqlCommand \n";

                result += Tabify(2) + "} // End of Try\n";

                result += Tabify(2) + "catch (Exception ex)\n";
                result += Tabify(2) + "{\n";
                result += Tabify(3) + "Console.WriteLine(ex.ToString());\n";
                result += Tabify(2) + "}\n";

                result += Tabify(2) + "cn.Close();\n";

                result += Tabify(1) + "} // End of Using (SqlConnection \n";

                result += Tabify(1) + "return result;\n";

                result += "} // End of GETTER\n";
            }
            return result;
        }

        [HttpGet]
        [Route("GetGetterWebMethodFromTableName")]
        public string GetGetterWebMethodFromTableName(
            string CN = "DBwSSPI_Login", string TNAME = "TNAME",
            string ClassName = "MyAwesomeObject", string FilterFieldName = "SomeFieldName", Boolean ReturnSingleton = false, Boolean GenerateInterfaceClass = true)

        {
            string SQLCode = "Select * from " + TNAME;

            string result = "";

            if (GenerateInterfaceClass)
                result = GetInterfaceClassFromSQLCode(CN, SQLCode, ClassName);

            List<CodeGenAPI.Models.Field> TheFields =
                (List<CodeGenAPI.Models.Field>)GetSchemaFieldsFromSQLCode(CN, SQLCode);

            var filterfieldtype = "string";
            var sqlfiltertype = "SqlDbType.VarChar";

            foreach (Field f in TheFields)
            {
                if (FilterFieldName.ToLower().Trim() == f.FieldName.ToLower())
                {
                    if (f.FieldType.ToLower().EndsWith("int32"))
                    {
                        filterfieldtype = "Int32";
                        sqlfiltertype = "SqlDbType.Int";
                    }

                    if (f.FieldType.ToLower().EndsWith("decimal"))
                    {
                        filterfieldtype = "Double";
                        sqlfiltertype = "SqlDbType.Float";
                    }

                    if (f.FieldType.ToLower().EndsWith("boolean"))
                    {
                        filterfieldtype = "Boolean";
                        sqlfiltertype = "SqlDbType.Bit";
                    }

                    if (f.FieldType.ToLower().EndsWith("datetime"))
                    {
                        filterfieldtype = "DateTime";
                        sqlfiltertype = "SqlDbType.DateTime";
                    }
                }
            }

            //foreach (CodeGenAPI.Models.Field theField in TheFields)
            //{
            //    result += TheTabs + theField.FieldType + " " + theField.FieldName + " { get; set; }\n";

            //}

            result += "\n\n\n";

            if (ReturnSingleton)
            {
                result += "[HttpGet]\n";
                result += "[Route(\"Get" + ClassName + "By" + FilterFieldName + "\")]\n";
                result += "public " + ClassName + " Get" + ClassName + "By" + FilterFieldName + " (" + filterfieldtype + " filt " + ")\n";
                result += "{\n";
                result += Tabify(1) + ClassName + " result = new " + ClassName + "();\n";
                result += Tabify(1) + "using (SqlConnection cn = new SqlConnection(\"" + FetchActualConnectionString(CN) + "\"))\n";
                result += Tabify(1) + "{\n";

                result += Tabify(2) + "try\n";
                result += Tabify(2) + "{\n";

                result += Tabify(3) + "cn.Open();\n";
                result += Tabify(3) + "var Sql = " + Stringify(SQLCode, 3);
                result += Tabify(3) + "Sql += \" where " + FilterFieldName + " = @filt\";\n";
                result += Tabify(3) + "using (SqlCommand cmd = new SqlCommand(Sql,cn))\n";
                result += Tabify(3) + "{\n";

                result += Tabify(4) + "cmd.Parameters.Add( \"@filt\"," + sqlfiltertype + ").Value = filt;\n";

                result += Tabify(4) + "cmd.CommandTimeout = 500;\n";
                result += Tabify(4) + "SqlDataReader r = cmd.ExecuteReader();\n";
                result += Tabify(4) + "while (r.Read())\n";
                result += Tabify(4) + "{\n";

                result += Tabify(5) + ClassName + " c = new " + ClassName + "();\n";

                foreach (Field f in TheFields)
                {
                    if (f.FieldType.ToLower().EndsWith("string"))
                        result += Tabify(5) + "c." + f.FieldName + " = r[\"" + f.FieldName + "\"] + \"\";\n";

                    if (f.FieldType.ToLower().EndsWith("boolean"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToBoolean(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToBoolean(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }

                    if (f.FieldType.ToLower().EndsWith("int32"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToInt32(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToInt32(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }

                    if (f.FieldType.ToLower().EndsWith("datetime"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToDateTime(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToDateTime(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }

                    if (f.FieldType.ToLower().EndsWith("decimal"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToDecimal(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToDecimal(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }


                }

                result += Tabify(5) + "result = c;\n";

                result += Tabify(4) + "} // End of While() \n";

                result += Tabify(3) + "cmd.Dispose();\n";

                result += Tabify(3) + "} // End of Using (SqlCommand \n";

                result += Tabify(2) + "} // End of Try\n";

                result += Tabify(2) + "catch (Exception ex)\n";
                result += Tabify(2) + "{\n";
                result += Tabify(3) + "Console.WriteLine(ex.ToString());\n";
                result += Tabify(2) + "}\n";

                result += Tabify(2) + "cn.Close();\n";

                result += Tabify(1) + "} // End of Using (SqlConnection \n";

                result += Tabify(1) + "return result;\n";

                result += "} // End of GETTER\n";
            }
            else
            {
                result += "[HttpGet]\n";
                result += "[Route(\"GetListOf" + ClassName + "By" + FilterFieldName + "\")]\n";
                result += "public List<" + ClassName + "> GetListOf" + ClassName + "By" + FilterFieldName + " (" + filterfieldtype + " filt " + ")\n";
                result += "{\n";
                result += Tabify(1) + "List<" + ClassName + "> result = new List<" + ClassName + ">();\n";
                result += Tabify(1) + "using (SqlConnection cn = new SqlConnection(\"" + FetchActualConnectionString(CN) + "\"))\n";
                result += Tabify(1) + "{\n";

                result += Tabify(2) + "try\n";
                result += Tabify(2) + "{\n";

                result += Tabify(3) + "cn.Open();\n";
                result += Tabify(3) + "var Sql = " + Stringify(SQLCode, 3);
                result += Tabify(3) + "Sql += \" where " + FilterFieldName + " = @filt\";\n";
                result += Tabify(3) + "using (SqlCommand cmd = new SqlCommand(Sql,cn))\n";
                result += Tabify(3) + "{\n";

                result += Tabify(4) + "cmd.Parameters.Add( \"@filt\"," + sqlfiltertype + ").Value = filt;\n";

                result += Tabify(4) + "cmd.CommandTimeout = 500;\n";
                result += Tabify(4) + "SqlDataReader r = cmd.ExecuteReader();\n";
                result += Tabify(4) + "while (r.Read())\n";
                result += Tabify(4) + "{\n";

                result += Tabify(5) + ClassName + " c = new " + ClassName + "();\n";

                foreach (Field f in TheFields)
                {
                    if (f.FieldType.ToLower().EndsWith("string"))
                        result += Tabify(5) + "c." + f.FieldName + " = r[\"" + f.FieldName + "\"] + \"\";\n";

                    if (f.FieldType.ToLower().EndsWith("boolean"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToBoolean(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToBoolean(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }

                    if (f.FieldType.ToLower().EndsWith("int32"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToInt32(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToInt32(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }

                    if (f.FieldType.ToLower().EndsWith("datetime"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToDateTime(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToDateTime(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }

                    if (f.FieldType.ToLower().EndsWith("decimal"))
                    {
                        if (f.AllowNulls)
                        {
                            result += Tabify(5) + "if (!Convert.IsDBNull(r[\"" + f.FieldName + "\"]))\n";
                            result += Tabify(5) + "{\n";
                            result += Tabify(6) + "c." + f.FieldName + " = Convert.ToDecimal(r[\"" + f.FieldName + "\"]);\n";
                            result += Tabify(5) + "}\n";
                        }
                        else
                        {

                            result += Tabify(5) + "c." + f.FieldName + " = Convert.ToDecimal(r[\"" + f.FieldName + "\"]);\n";

                        }
                    }


                }

                result += Tabify(5) + "result.Add(c);\n";

                result += Tabify(4) + "} // End of While() \n";

                result += Tabify(3) + "cmd.Dispose();\n";

                result += Tabify(3) + "} // End of Using (SqlCommand \n";

                result += Tabify(2) + "} // End of Try\n";

                result += Tabify(2) + "catch (Exception ex)\n";
                result += Tabify(2) + "{\n";
                result += Tabify(3) + "Console.WriteLine(ex.ToString());\n";
                result += Tabify(2) + "}\n";

                result += Tabify(2) + "cn.Close();\n";

                result += Tabify(1) + "} // End of Using (SqlConnection \n";

                result += Tabify(1) + "return result;\n";

                result += "} // End of GETTER\n";
            }
            return result;
        }


        [HttpGet]
        [Route("GetSchemaOfSQLCode")]
        public string GetSchemaOfSQLCode(string CN = "DBwSSPI_Login",string SQLCode = "Select top 1 * from SOMETABLE")
        {
            string result = "";

            CN = FetchActualConnectionString(CN);

            SqlConnection cn = new SqlConnection(CN);

            SqlCommand cmd = new SqlCommand(SQLCode, cn);

            SqlDataAdapter ad = new SqlDataAdapter(cmd);

            DataSet ds = new DataSet();
            ad.FillSchema(ds, SchemaType.Mapped);

            var metadata = ds.Tables[0];

            foreach(DataColumn col in metadata.Columns)
            {
                result += col.ColumnName + " - " + col.DataType + " - " + col.AllowDBNull.ToString() + " - " + col.AutoIncrement + "\n";
            }

            ds.Dispose();
            ad.Dispose();
            cmd.Dispose();
            cn.Close();
            cn.Dispose();



            return result;
        }

        [HttpGet]
        [Route("GetIdentityFieldForTable")]
        public string GetIdentityFieldForTable(string CN = "DBwSSPI_Login", string TableName = "SOMETABLE")
        {
            string result = "Table has no Identity Field";

            CN = FetchActualConnectionString(CN);

            SqlConnection cn = new SqlConnection(CN);

            string SQLCode = "Select top 1 * from [" + TableName + "]";


            SqlCommand cmd = new SqlCommand(SQLCode, cn);

            SqlDataAdapter ad = new SqlDataAdapter(cmd);

            DataSet ds = new DataSet();
            try { 
                ad.FillSchema(ds, SchemaType.Mapped);
                var metadata = ds.Tables[0];

                foreach (DataColumn col in metadata.Columns)
                {
                    if (col.AutoIncrement)
                    {
                        result = col.ColumnName;
                        break;
                    }

                    //result += col.ColumnName + " - " + col.DataType + " - " + col.AllowDBNull.ToString() + " - " + col.AutoIncrement + "\n";
                }
            }
            catch
            {
                result = "Table Likely does not exist";
            }
            
            

            ds.Dispose();
            ad.Dispose();
            cmd.Dispose();
            cn.Close();
            cn.Dispose();



            return result;
        }

        [HttpGet]
        [Route("GetListofTablePKs")]
        public List<TablesAndPKs> GetListOFTablePKs(string CN = "DBwSSPI_Login")
        {
            List<TablesAndPKs> result = new List<TablesAndPKs>();

            var thestring = "";
            thestring += "select schema_name(tab.schema_id) as [schema_name], ";
            thestring += "    pk.[name] as pk_name, ";
            thestring += "    substring(column_names, 1, len(column_names)-1) as [columns], ";
            thestring += "    tab.[name] as table_name ";
            thestring += "from sys.tables tab ";
            thestring += "    inner join sys.indexes pk ";
            thestring += "        on tab.object_id = pk.object_id ";
            thestring += "        and pk.is_primary_key = 1 ";
            thestring += "   cross apply (select col.[name] + ', ' ";
            thestring += "                    from sys.index_columns ic ";
            thestring += "                        inner join sys.columns col ";
            thestring += "                            on ic.object_id = col.object_id ";
            thestring += "                            and ic.column_id = col.column_id ";
            thestring += "                    where ic.object_id = tab.object_id ";
            thestring += "                        and ic.index_id = pk.index_id ";
            thestring += "                            order by col.column_id ";
            thestring += "                            for xml path ('') ) D (column_names) ";
            thestring += "order by schema_name(tab.schema_id), ";
            thestring += "    pk.[name]";

            CN = FetchActualConnectionString(CN);

            SqlConnection cn = new SqlConnection(CN);
            cn.Open();

            SqlCommand cmd = new SqlCommand(thestring, cn);

            SqlDataReader r = cmd.ExecuteReader();

            while (r.Read())
            {
                TablesAndPKs tp = new TablesAndPKs();

                tp.TableName = r["table_Name"] + "";
                tp.PKName = r["columns"] + "";

                result.Add(tp);
            }

            r.Close();
            cmd.Dispose();
            cn.Close();
            cn.Dispose();

            return result;
        }

        [HttpGet]
        [Route("GetListOfOtherTableKeys")]
        public List<OtherTableKeys> GetListOfOtherTableKeys(string CN = "DBwSSPI_Login", string TNAME = "")
        {
            List<OtherTableKeys> result = new List<OtherTableKeys>();

            List<string> ColsInTable = (List<string>)GetTableColumns(CN, TNAME);
            List<TablesAndPKs> tp = GetListOFTablePKs(CN);

            foreach (string col in ColsInTable)
            {
                foreach (TablesAndPKs tp2 in tp)
                {
                    if(tp2.PKName == col && tp2.TableName.ToLower() != TNAME.ToLower())
                    {
                        OtherTableKeys otk = new OtherTableKeys();
                        otk.TableName = tp2.TableName;
                        otk.PKName = tp2.PKName;

                        result.Add(otk);

                    }
                }
            }


            return result;
        }

        [HttpGet]
        [Route("GetFullGettersForGivenTableName")]
        public string GetFullGettersForGivenTableName(string CN = "DBwSSPI_Login", string TNAME = "SomeTable")
        {
            string result = "";

            List<TablesAndPKs> tpks = GetListOFTablePKs(CN);
            TablesAndPKs tpk = new TablesAndPKs();

            foreach (TablesAndPKs tp2 in tpks)
            {
                if (tp2.TableName.ToLower() == TNAME.ToLower())
                {
                    tpk = tp2;
                    break;
                }
            }


            List<OtherTableKeys> otks = GetListOfOtherTableKeys(CN, TNAME);

            result = GetGetterWebMethodFromTableName(CN, TNAME, TNAME, tpk.PKName, true, true);

            foreach(OtherTableKeys otk in otks)
            {
                result += "\n";
                result += GetGetterWebMethodFromTableName(CN, TNAME, TNAME, otk.PKName, false, false);
            }

            return result;
        }

        [HttpGet]
        [Route("GetPOSTMethodForTable")]
        public string GetPOSTMethodForTable(string CN = "DBwSSPI_Login", string TNAME = "SomeTable", Boolean GenerateInterfaceClass = true)
        {
            string result = "";

            string TheTabs = "";

            //Cast it to a LIST because its just easier to work with that rater than a crappy Ienumerable
            List<Models.Field> TheFields = (List<Models.Field>)GetTableSchemaFields(CN, TNAME);

            if (GenerateInterfaceClass)
                result = GetInterfaceClassFromSpecificTableName(CN, TNAME, TNAME) + "\n";

            result += "[HttpPost]\n";
            result += "[Route(\"Post" + TNAME +"\")]\n";
            result += "public void Post" + TNAME + "([FromBody] " + TNAME + " value )\n";
            result +="{\n\n";

            TheTabs = "\t";
            result += TheTabs + "string SQL = \"\";\n";

            // Figure out what field is the ID field if any
            foreach(Models.Field theField in TheFields)
            {
                if (theField.IsIdentity)
                {
                    // we have an Identity field so we need to make the SQL being built
                    // aware the ID fields might be already set meaning and UPdate
                    // otherwise it needs to be crafted as an INSERT INTO.

                    result += TheTabs + "if (value." + theField.FieldName + ">0)\n" +
                        TheTabs + "{\n";

                    TheTabs = "\t\t";

                    result += TheTabs + "SQL = \"Insert Into [" + TNAME + "] (";
                    
                    foreach(Models.Field f1 in TheFields)
                    {
                        if (!f1.IsIdentity)
                        {
                            result += "[" + f1.FieldName + "],";
                        }
                    }

                    // Strip off the Trailing "," character

                    result = result.Substring(0,result.Length - 1);

                    result += ") \" +\n" +
                        TheTabs + "\"Values (";

                    foreach (Models.Field f1 in TheFields)
                    {
                        if (!f1.IsIdentity)
                        {
                            result += "@" + f1.FieldName + ",";
                        }
                    }

                    // Strip off the Trailing "," character

                    result = result.Substring(0, result.Length - 1);

                    result += ") \"; \n";



                   
                    TheTabs = "\t";
                    result += TheTabs + "}\n";
                }
            }




            result += "}\n\n";
            
            return result;
        }

        

        #region Private Stuff

        private string FetchActualConnectionString(string TheKey)
        {
            string result = "";
            var _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var things = _config.GetSection("Settings:Databases").GetChildren();

            foreach (var i in things)
            {
                if (i.Key.ToLower() == TheKey.ToLower())
                {
                    result = i.Value;
                    break;
                }
            }

            return result.ToString();
        }

        private string DoTheIndentation(string code)
        {
            StringBuilder sb = new StringBuilder();

            string[] lines = code.Split("\n".ToCharArray());

            int indent = 0;

            foreach (string s in lines)
            {
                string[] OPENBRAKS = s.Split("{".ToCharArray());
                string[] CLOSEBRAKS = s.Split("}".ToCharArray());

                // if line starts with the # character don't do the indent

                if (s.StartsWith("#region") || s.StartsWith("#endregion"))
                {
                    sb.Append(INDENT(indent) + s + "\n");

                    // handle indentation after appending code line
                    //indent += OPENBRAKS.GetUpperBound(0);
                    //indent -= CLOSEBRAKS.GetUpperBound(0);
                }
                else
                {

                    if (s == "}")
                    {
                        // handle indentation before the code line as the line is a trailing } 
                        indent += OPENBRAKS.GetUpperBound(0);
                        indent -= CLOSEBRAKS.GetUpperBound(0);

                        sb.Append(INDENT(indent) + s + "\n");
                    }
                    else
                    {
                        sb.Append(INDENT(indent) + s + "\n");

                        // handle indentation after appending code line
                        indent += OPENBRAKS.GetUpperBound(0);
                        indent -= CLOSEBRAKS.GetUpperBound(0);
                    }
                }
            }
            return sb.ToString();
        }

        private string INDENT(int indent)
        {
            string s = "";

            for (int t = 0; t < indent; t++)
            {
                s += "\t";
            }

            return s;

        }

        private string GeneratePrivateMethods()
        {
            string s = "";

            if (AUTONUMBER)
            {
                #region AUTONUMBER STUFF

                s = "#region Private Methods\n\n" +
                    "private string GetParameterSQL() {\n" +
                    "string sql = \"\";\n";

                s += "if (_" + IDFIELDNAME + " < 1) {\n" +
                     "sql = \"INSERT INTO " + TableName + "\";\n";

                s += "sql += \"(\";\n";

                string temps = "";

                string lastfield = TheFields[TheFields.Count - 1].FieldName;

                foreach (Field f in TheFields)
                {
                    if (!f.IsIdentity)
                    {
                        if (temps != "")
                        {
                            temps += "], [" + f.FieldName;
                        }
                        else
                        {
                            temps += "[" + f.FieldName;
                        }

                        if (temps.Length > 70 && f.FieldName != lastfield) // create a line break but NOT if we are on the LASTFIELD
                        {
                            // we want a new line

                            s += "sql += \"" + temps + "],\";\n";
                            temps = "";
                        }
                    }
                }

                // do we have any hanging fields at the end

                if (temps != "")
                {
                    s += "sql += \"" + temps + "])\";\n";
                }
                else
                {

                    if (s.EndsWith(",]\";\n"))
                    {
                        // we inadvertantly ended in a mess from the loop above we need to fix it

                        int t = ",]\";\n".Length;

                        s = s.Substring(0, s.Length - t);
                    }

                    s += "sql += \"]) \";\n";
                }

                s += "sql += \" VALUES (\";\n";

                // Write the Values as Parameters

                temps = "";

                foreach (Field f in TheFields)
                {
                    if (!f.IsIdentity)
                    {
                        if (temps != "")
                        {
                            temps += ",@" + f.FieldNameConverted;
                        }
                        else
                        {
                            temps += "@" + f.FieldNameConverted;
                        }

                        if (temps.Length > 70)
                        {
                            // we want a new line

                            s += "sql += \"" + temps + ",\";\n";
                            temps = "";
                        }
                    }
                }

                // do we have any hanging fields at the end

                if (temps != "")
                {
                    s += "sql += \"" + temps + ")\";\n";
                }
                else
                {

                    if (s.EndsWith(",\";\n"))
                    {
                        // we unadvertantly ended in a mess from the loop above we need to fix it

                        int t = ",\";\n".Length;

                        s = s.Substring(0, s.Length - t) + "\";\n";
                    }

                    s += "sql += \") \";\n";
                }

                s += "} else {\n";

                s += "sql = \"UPDATE " + TableName + " SET \";\n";

                temps = "";

                foreach (Field f in TheFields)
                {
                    if (!f.IsIdentity)
                    {
                        if (temps != "")
                        {
                            temps += ", [" + f.FieldName + "] = @" + f.FieldNameConverted;
                        }
                        else
                        {
                            temps += "[" + f.FieldName + "] = @" + f.FieldNameConverted;
                        }

                        if (temps.Length > 70)
                        {
                            // we want a new line

                            s += "sql += \"" + temps + ",\";\n";
                            temps = "";
                        }
                    }
                }

                // do we have any hanging fields at the end

                if (temps != "")
                {
                    s += "sql += \"" + temps + "\";\n";
                }
                else
                {
                    if (s.EndsWith(",\";\n"))
                    {
                        // we unadvertantly ended in a mess from the loop above we need to fix it

                        int t = ",\";\n".Length;

                        s = s.Substring(0, s.Length - t) + "\";\n";
                    }

                    s += "sql += \"\";\n";
                }

                s += "sql += \" WHERE " + IDFIELDNAME + " = \" + _" + IDFIELDNAME + ".ToString();\n";
                s += "}\n";
                s += "return sql;\n";
                s += "}\n\n";

                s += "private object getDateOrNull(DateTime d)\n";
                s += "{\n";
                s += "if ( d == Convert.ToDateTime(null)) {\n";
                s += "return DBNull.Value;\n";
                s += "} else {\n";
                s += "return d;\n";
                s += "}\n";
                s += "}\n";
                s += "#endregion\n";


                #endregion

            }
            else
            {

                #region Non AutoNumber stuff

                // We are not autonumbering so we have to do some different things here

                s = "#region Private Methods\n\n" +
                    "private string GetParameterSQLForAdd() {\n" +
                    "string sql = \"\";\n";

                s += "if (1==1) { // A Hack I suppose but it works...\n" +
                     "sql = \"INSERT INTO " + TableName + "\";\n";

                s += "sql += \"(\";\n";

                string temps = "";

                string lastfield = TheFields[TheFields.Count - 1].FieldName;

                foreach (Field f in TheFields)
                {
                    if (!f.IsIdentity)
                    {
                        if (temps != "")
                        {
                            temps += "], [" + f.FieldName;
                        }
                        else
                        {
                            temps += "[" + f.FieldName;
                        }

                        if (temps.Length > 70 && f.FieldName != lastfield) // create a line break but NOT if we are on the LASTFIELD
                        {
                            // we want a new line

                            s += "sql += \"" + temps + "],\";\n";
                            temps = "";
                        }
                    }
                }

                // do we have any hanging fields at the end

                if (temps != "")
                {
                    s += "sql += \"" + temps + "])\";\n";
                }
                else
                {

                    if (s.EndsWith(",]\";\n"))
                    {
                        // we inadvertantly ended in a mess from the loop above we need to fix it

                        int t = ",]\";\n".Length;

                        s = s.Substring(0, s.Length - t);
                    }

                    s += "sql += \"]) \";\n";
                }

                s += "sql += \" VALUES (\";\n";

                // Write the Values as Parameters

                temps = "";

                foreach (Field f in TheFields)
                {
                    if (!f.IsIdentity)
                    {
                        if (temps != "")
                        {
                            temps += ",@" + f.FieldNameConverted;
                        }
                        else
                        {
                            temps += "@" + f.FieldNameConverted;
                        }

                        if (temps.Length > 70)
                        {
                            // we want a new line

                            s += "sql += \"" + temps + ",\";\n";
                            temps = "";
                        }
                    }
                }

                // do we have any hanging fields at the end

                if (temps != "")
                {
                    s += "sql += \"" + temps + ")\";\n";
                }
                else
                {

                    if (s.EndsWith(",\";\n"))
                    {
                        // we unadvertantly ended in a mess from the loop above we need to fix it

                        int t = ",\";\n".Length;

                        s = s.Substring(0, s.Length - t) + "\";\n";
                    }

                    s += "sql += \") \";\n";
                }

                s += "}\n";

                s += "return sql;\n";
                s += "}\n\n";


                s += "private string GetParameterSQLForUpdate() {\n" +
                   "string sql = \"\";\n";

                s += "sql = \"UPDATE " + TableName + " SET \";\n";

                temps = "";

                foreach (Field f in TheFields)
                {
                    if (!f.IsIdentity)
                    {
                        if (temps != "")
                        {
                            temps += ", [" + f.FieldName + "] = @" + f.FieldNameConverted;
                        }
                        else
                        {
                            temps += "[" + f.FieldName + "] = @" + f.FieldNameConverted;
                        }

                        if (temps.Length > 70)
                        {
                            // we want a new line

                            s += "sql += \"" + temps + ",\";\n";
                            temps = "";
                        }
                    }
                }

                // do we have any hanging fields at the end

                if (temps != "")
                {
                    s += "sql += \"" + temps + "\";\n";
                }
                else
                {
                    if (s.EndsWith(",\";\n"))
                    {
                        // we inadvertantly ended in a mess from the loop above we need to fix it

                        int t = ",\";\n".Length;

                        s = s.Substring(0, s.Length - t) + "\";\n";
                    }

                    s += "sql += \"\";\n";
                }

                // If the field type of the IDFIELD is a number we dont quote it otherwise its quoted
                if (IDFIELDTYPE == "BIGINT" || IDFIELDTYPE == "LONG" || IDFIELDTYPE == "INT" || IDFIELDTYPE == "SMALLINT" || IDFIELDTYPE == "TINYINT" ||
                    IDFIELDTYPE == "DECIMAL" || IDFIELDTYPE == "DOUBLE" || IDFIELDTYPE == "MONEY" || IDFIELDTYPE == "CURRENCY" || IDFIELDTYPE == "FLOAT")
                {
                    //s += "sql += \" WHERE " + IDFIELDNAME + " = \" + _" + IDFIELDNAME + ".ToString();\n";

                    s += "sql += \" WHERE [" + IDFIELDNAME + "] = @" + IDFIELDNAME + "\";\n";

                }
                else
                {
                    s += "sql += \" WHERE [" + IDFIELDNAME + "] = @" + IDFIELDNAME + "\";\n";
                }

                s += "return sql;\n";
                s += "}\n\n";

                s += "private object getDateOrNull(DateTime d)\n";
                s += "{\n";
                s += "if ( d == Convert.ToDateTime(null)) {\n";
                s += "return DBNull.Value;\n";
                s += "} else {\n";
                s += "return d;\n";
                s += "}\n";
                s += "}\n";
                s += "#endregion\n";

                #endregion

            }
            return s;
        }

        private string Tabify(int numtabs)
        {
            string result = "";

            for (int i= 0; i < numtabs; i++)
            {
                result += "\t";

            }

            return result;
        }

        private string Stringify(string TheString, int tcount)
        {
            string[] arr = TheString.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            StringBuilder s = new StringBuilder();

            //s.Append(Tabify(tcount) + "var thestring = \"\";\n");

            bool first = true;

            foreach (string ss in arr)
            {
                if (first)
                    s.Append("\"" + ss + "\" + \n");
                else
                    s.Append(Tabify(tcount) + "\"" + ss + "\" + \n");

            }

            s.Append(Tabify(tcount) + "\"\";\n");

            return s.ToString();
            
        }

        #endregion

        #region InterfaceClasses

        

        #endregion
    }

    public class TablesAndPKs
    {
        public string TableName { get; set; } = "";
        public string PKName { get; set; } = "";
    }

    public class OtherTableKeys
    {
        public string TableName { get; set; } = "";
        public string PKName { get; set; } = "";
    }
}

