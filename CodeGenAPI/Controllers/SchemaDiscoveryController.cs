﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.Management.Common;

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
    using Swashbuckle.AspNetCore.Annotations;
    using System.Text;
    using System.Configuration;
    using System.Data;
    using Newtonsoft.Json.Linq;


    ///<Summary>
    /// The SchemaDiscoveryController class is used to discover the schema of the database
    ///</Summary>
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

        ///<Summary>
        /// The GetAll method is used to enumerate all Tables and Views and pertinate information about all those entities
        ///</Summary>
        [HttpGet]
        [Route("GetAll")]
        [SwaggerOperation(Summary = "Will attempt to enumerate all Tables and Views and pertinate information about all those entities " +
                                    " in the database. Returning a JSON object with the structure. " +
                                    "On large databases this might take some time to execute and may timeout")]
        public string GetAll(string CN = "DBwSSPI_Login")
        {
            // On big databases this will likely cause timeouts and other squirrly behaiviors for tool like the CrapTacular SWAGGER
            string result = "RESULT";

            CN = FetchActualConnectionString(CN);

            SqlConnection cn = new SqlConnection(CN);

            cn.Open();

            string sqlstring = "select A.*," +
                            "B.TABLE_TYPE," +
                            "(SELECT OBJECT_ID(A.TABLE_NAME)) as TABLEID," +
                            "(SELECT IS_IDENTITY FROM SYS.columns SC WHERE SC.object_id = (SELECT OBJECT_ID(A.TABLE_NAME)) AND SC.NAME = A.COLUMN_NAME  ) as IS_IDENTITY " +
                            "from INFORMATION_SCHEMA.COLUMNS as A " +
                            "LEFT OUTER JOIN " +
                            "INFORMATION_SCHEMA.TABLES B on A.TABLE_NAME = B.TABLE_NAME " +
                            "ORDER BY A.TABLE_NAME,A.ORDINAL_POSITION";


            SqlDataAdapter da = new SqlDataAdapter(sqlstring, cn);

            DataSet d = new DataSet();

            da.Fill(d);

            result = JsonConvert.SerializeObject(d, Formatting.Indented);

            d.Dispose();
            da.Dispose();
            cn.Close();
            cn.Dispose();

            return result;
        }

        ///<Summary>
        /// The GetTables method is used to return a list of TABLE objects containing The DATABASE, The SCHEMA, The OBJECT Name, and its type (VIEW or BASE TABLE)
        ///</Summary>
        [HttpGet]
        [Route("GetTables")]
        [SwaggerOperation(Summary = "Will Return a list of TABLE objects containing The DATABASE, The SCHEMA, The OBJECT Name, and its type (VIEW or BASE TABLE)")]
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

        ///<Summary>
        /// The GetListOfTables method is used to return a list of all Tables and Views as an array of Strings with their names only
        ///</Summary>
        [HttpGet]
        [Route("GetListOfTables")]
        [SwaggerOperation(Summary = "Returns all Tables and Views as an array of Strings with their names only")]
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

        ///<Summary>
        /// The GetTableSchema method is used to return the Schema of a Table or View TN as a JSON Array of Schema objects
        ///</Summary>
        [HttpGet]
        [Route("GetTableSchema")]
        [SwaggerOperation(Summary = "Returns the Schema of a Table or View TN as a JSON Array of Schema objects")]
        public string GetTableSchema(
            string CN = "DBwSSPI_Login",
            string TN = "MemberMain")
        {
            string result = "RESULT";

            CN = FetchActualConnectionString(CN);

            SqlConnection cn = new SqlConnection(CN);


            cn.Open();

            var sqlstring = "select DISTINCT A.*," +
                            "B.TABLE_TYPE," +
                            "(SELECT OBJECT_ID(A.TABLE_NAME)) as TABLEID," +
                            "(SELECT IS_IDENTITY FROM SYS.columns SC WHERE SC.object_id = (SELECT OBJECT_ID(A.TABLE_NAME)) AND SC.NAME = A.COLUMN_NAME  ) as IS_IDENTITY " +
                            "from INFORMATION_SCHEMA.COLUMNS as A " +
                            "LEFT OUTER JOIN " +
                            "INFORMATION_SCHEMA.TABLES B on A.TABLE_NAME = B.TABLE_NAME " +
                            "WHERE A.TABLE_NAME = @TABLENAME AND A.TABLE_SCHEMA = 'dbo' " +
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

        ///<Summary>
        /// The GetTableSchemaFields method is used to return the array of Field Objects for each field in the Table or View TN
        ///</Summary>
        [HttpGet]
        [Route("GetTableSchemaFields")]
        [SwaggerOperation(Summary = "Returns the array of Field Objects for each field in the Table or View TN ")]
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

                var sqlstring = "select DISTINCT A.*," +
                                "B.TABLE_TYPE," +
                                "(SELECT OBJECT_ID(A.TABLE_NAME)) as TABLEID," +
                                "(SELECT IS_IDENTITY FROM SYS.columns SC WHERE SC.object_id = (SELECT OBJECT_ID(A.TABLE_NAME)) AND SC.NAME = A.COLUMN_NAME  ) as IS_IDENTITY " +
                                "from INFORMATION_SCHEMA.COLUMNS as A " +
                                "LEFT OUTER JOIN " +
                                "INFORMATION_SCHEMA.TABLES B on A.TABLE_NAME = B.TABLE_NAME " +
                                "WHERE A.TABLE_NAME = @TABLENAME AND A.TABLE_SCHEMA = 'dbo' " +
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

        ///<Summary>
        /// The GetTableModel method is used to return the code to encapsulate reading, writing, updating and deleting records from the indicated TableName
        ///</Summary>
        [HttpGet]
        [Route("GetTableModel")]
        [SwaggerOperation(Summary = "Creates code to encapsulate reading, writing, updating and deleting records from the indicated TableName")]
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

        ///<Summary>
        /// The GetTableColumns method will return an array of strings containing all the columns in the supplied Tablename TN
        ///</Summary>
        [HttpGet]
        [Route("GetTableColumns")]
        [SwaggerOperation(Summary = "Will return an array of strings containing all the columns in the supplied Tablename TN")]
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

        ///<Summary>
        /// The GetConfiguration method will return a list of all the databases named and configured in this installations app.config. These named keywork=value pairs are used for all the other calls in the CN parameter
        ///</Summary>
        [HttpGet]
        [Route("GetConfiguration")]
        [SwaggerOperation(Summary = "Will return a list of all the databases named and configured in this installations app.config. These named keywork=value pairs are used for all the other calls in the CN parameter")]
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

        ///<Summary>
        /// The GetSchemaFieldsFromSQLCode method will return an array of objects the enumerate many characteristics of the fields resuling from the supplied query  SQLCode allowNulls, fieldName fieldNameConverted, isIdentity, maxLength, precision,crosswalk, crosswalktable,crosswalkvalue,crosswalkdisplay, tablename
        ///</Summary>
        [HttpGet]
        [Route("GetSchemaFieldsFromSQLCode")]
        [SwaggerOperation(Summary = "Will return an array of objects the enumerate many characteristics " +
                                    "of the fields resuling from the supplied query  SQLCode " +
                                    "allowNulls, fieldName fieldNameConverted, isIdentity, maxLength, precision," +
                                    "crosswalk, crosswalktable,crosswalkvalue,crosswalkdisplay, tablename")]    
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

            for (int i = 0; i < ds.Tables.Count; i++)
            {

                var metadata = ds.Tables[i];

                foreach (DataColumn col in metadata.Columns)
                {
                    Field f = new Field();

                    f.AllowNulls = col.AllowDBNull;
                    f.FieldName = col.ColumnName;
                    f.FieldType = col.DataType.ToString();
                    f.IsIdentity = col.AutoIncrement;
                    f.MaxLength = col.MaxLength;
                    f.Precision = 0;
                    f.TABLENAME = ds.Tables[i].TableName;
                    

                    result.Add(f);


                }
            }

            ds.Dispose();
            ad.Dispose();
            cmd.Dispose();
            cn.Close();
            cn.Dispose();

            SqlConnection conn = new SqlConnection(CN);
            conn.Open();

            cmd = new SqlCommand(SQLCode, conn);

            string TableName = "Table";
            string RESTableName = TableName;
            
            List<string> Thelist = new List<string>();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                int resultindex = 0;
                do
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    
                    //Thelist.Add("Results Index " + resultindex.ToString());

                    if (dt.Rows.Count > 1)
                    {
                        foreach (Field f in result)
                        {
                            if (f.TABLENAME.ToLower() == RESTableName.ToLower())
                            {
                                f.IsMultiple = true;
                            }
                        }
                    }

                    resultindex += 1;

                    RESTableName = TableName + resultindex.ToString().Trim();
                        
                }
                while (!reader.IsClosed && reader.Read()); // Move to the next result set 
            }
            cmd.Dispose();
            conn.Close();
            conn.Dispose();

            return result;
        }

        ///<Summary>
        /// The GetInterfaceClassFromSQLCode method will return a Lightweight Data Object encapsulating the Scheme of a supplied query against the database LDO Will be named <ClassName>
        ///</Summary>
        [HttpGet]
        [Route("GetInterfaceClassFromSQLCode")]
        [SwaggerOperation(Summary = "Will return a Lightweight Data Object encapsulating the Scheme of a supplied query against the database LDO Will be named <ClassName>")]
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


        ///<Summary>
        /// The GetInterfaceClassFromSpecificTableName method will return a Lightweight Data Object encapsulating the Schema of a supplied table or view <TNAME> the returned class will be named <ClassName>
        ///</Summary>
        [HttpGet]
        [Route("GetInterfaceClassFromSpecificTableName")]
        [SwaggerOperation(Summary = "Will return a Lightweight Data Object encapsulating the Schema of a supplied table or view <TNAME> the returned class will be named <ClassName>")]
        public string GetInterfaceClassFromSpecificTableName(
            string CN = "DBwSSPI_Login", string Tname = "TNAME", string ClassName = "MyAwesomeObject")
        {

            return GetInterfaceClassFromSpecificTableNameInternal(CN, Tname, ClassName, true);
        }

        private string GetInterfaceClassFromSpecificTableNameInternal(
            string CN = "DBwSSPI_Login", string Tname = "TNAME", string ClassName = "MyAwesomeObject", Boolean DoTab = true)
        {
            string result = "public class " + ClassName + "\n" + "{\n";

            string TheTabs = "\t";
            
            if (!DoTab)
                TheTabs = "";
            
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

        ///<Summary>
        /// The GetAllTablesInterfaceClassesFromDataBase method will return a Lightweight Data Object encapsulating the Schema of each table and view in the target database
        ///</Summary>
        [HttpGet]
        [Route("GetAllTablesInterfaceClassesFromDataBase")]
        [SwaggerOperation(Summary = "Will retrieve Lightweight Data Object classes for each table and View in the target database. NOTE: This is a LONG running process so use it with Caution")]
        public string GetAllTablesInterfaceClassesFromDataBase(
            string CN = "DBwSSPI_Login")
        {
            StringBuilder result = new StringBuilder();

            IEnumerable<String> TheTables = GetListOfTables(CN);

            foreach(string TableName in TheTables)
            {

                try
                {
                    result.Append(GetInterfaceClassFromSQLCode(CN, "SELECT TOP 1 * from [" + TableName + "]", "cls" + TableName));
                
                    result.Append(System.Environment.NewLine);
                }
                catch (Exception e)
                {
                    result.Append("/*\n");
                    result.Append(e.Message);
                    result.Append("\n*/\n");
                }
                
                //result.Append(GetInterfaceClassFromSQLCode(CN, "SELECT TOP 1 * from [" + TableName + "]", "cls" + TableName));
                
                //result.Append(System.Environment.NewLine);
            }


            return result.ToString();
        }

        ///<Summary>
        /// The GetGetterWebMethodFromSQLCode method will return a Lightweight Data Object encapsulating the Schema of a supplied query against the database LDO Will be named <ClassName>
        ///</Summary>
        [HttpGet]
        [Route("GetGetterWebMethodFromSQLCode")]
        [SwaggerOperation(Summary = "Given a specific Query SQLCode\n" + 
                                    "Will return a Lightweight data object named Classname if GenerateInterfaceClass if True,\n" +
                                    " as well as a restful endpoint code that will return\n" +
                                    "as List<ClassName> or a single ClassName object. Dependant on returnsingleton")]
        public string GetGetterWebMethodFromSQLCode(
            string CN = "DBwSSPI_Login", string SQLCode = "Select top 1 * from SOMETABLE", 
            string ClassName = "MyAwesomeObject", string FilterFieldName = "SomeFieldName", Boolean ReturnSingleton = false,
            Boolean MakeItAsync = false)
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

                if (MakeItAsync)
                {
                    result += "public async Task<" + ClassName + "> Get" + ClassName + "async (" + filterfieldtype + " filt " + ")\n";
                    result += "{\n";
                    result += Tabify(1) + "return await Task.Run(() => {\n";
                }
                else
                {
                    result += "public " + ClassName + " Get" + ClassName + " (" + filterfieldtype + " filt " + ")\n";
                    result += "{\n";
                }

                //result += "public " + ClassName + " Get" + ClassName + " (" + filterfieldtype + " filt " + ")\n";
                //result += "{\n";
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

                    if (f.FieldType.ToLower().EndsWith("decimal") || f.FieldType.ToLower().EndsWith("double"))
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

                if (MakeItAsync)
                {
                    result += Tabify(1) + "});\n";
                }

                result += "} // End of GETTER\n";
            }
            else
            {
                result += "[HttpGet]\n";
                result += "[Route(\"GetListOf" + ClassName + "\")]\n";

                if (MakeItAsync)
                {
                    result += "public async Task<List<" + ClassName + ">> GetListOf" + ClassName + "async (" + filterfieldtype + " filt " + ")\n";
                    result += "{\n";
                    result += Tabify(1) + "return await Task.Run(() => {\n";
                }
                else
                {
                    result += "public List<" + ClassName + "> GetListOf" + ClassName + " (" + filterfieldtype + " filt " + ")\n";
                    result += "{\n";
                }

                //result += "public List<" + ClassName + "> GetListOf" + ClassName + " (" + filterfieldtype + " filt " + ")\n";
                //result += "{\n";
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

                    if (f.FieldType.ToLower().EndsWith("decimal") || f.FieldType.ToLower().EndsWith("double"))
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

                if (MakeItAsync)
                {
                    result += Tabify(1) + "});\n";
                }

                result += "} // End of GETTER\n";
            }
            return result;
        }

        ///<Summary>
        /// The GetGetterWebMethodFromTableName method will return a restful endpoint code that will return a lightweight data object named Classname if GenerateInterfaceClass if True, as well as a restful endpoint code that will return as List<ClassName> or a single ClassName object. Dependant on returnsingleton
        ///</Summary>
        [HttpGet]
        [Route("GetGetterWebMethodFromTableName")]
        [SwaggerOperation(Summary = "Given a TableName TNAME\n" +
            "Will return a Lightweight data object named Classname if GenerateInterfaceClass if True,\n" +
            " as well as a restful endpoint code that will return\n" +
            "as List<ClassName> or a single ClassName object. Dependant on returnsingleton")]
        public string GetGetterWebMethodFromTableName(
            string CN = "DBwSSPI_Login", string TNAME = "TNAME",
            string ClassName = "MyAwesomeObject", string FilterFieldName = "SomeFieldName", Boolean ReturnSingleton = false, Boolean GenerateInterfaceClass = true)
        {
            string SQLCode = "Select * from " + TNAME;

            string result = "";

            if (GenerateInterfaceClass)
            {
                result += "\n//-----------------------------------------------------------------\n";
                result += "//-------- Interface Class                                    -----\n";
                result += "//-----------------------------------------------------------------\n\n";

                result += GetInterfaceClassFromSQLCode(CN, SQLCode, ClassName);
            }

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

            result += "\n";

            result += "\n//-----------------------------------------------------------------\n";
            result += "//--------  Getter for Item based on Field Named " + FilterFieldName + "\n";
            result += "//-----------------------------------------------------------------\n\n";

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

                    if (f.FieldType.ToLower().EndsWith("decimal") || f.FieldType.ToLower().EndsWith("double"))
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

                    if (f.FieldType.ToLower().EndsWith("decimal") || f.FieldType.ToLower().EndsWith("double"))
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
        [SwaggerOperation(Summary =
            "Will take a simple Query string and return a formatted list string with FieldName - Date Type - ISIdentity Bool - AllowNull Bool each on their own line")]
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
        [SwaggerOperation(Summary =
            "Will return a single string with the name of the Identity Field defined for TableName.\n" +
            "If its a View and there is an Identity field on the base table used in the view That will\n" +
            "be returned as the result.")]
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
        [SwaggerOperation(Summary =
            "Will return an array of Tuples containing TableName and PkName for those tables.")]
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
            thestring += "Where schema_name(tab.schema_id) = 'dbo' ";
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
        [SwaggerOperation(Summary =
            "Will return a list of fields in the given table that match name and type of field in other tables in the database.\n" +
            "This might normally be defined as a Foreign Key field but often will not be actually defined at the database level as such\n" +
            "Still its use as such might be inferred. This INFO might prove useful in some cases.")]
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
        [SwaggerOperation(Summary =
            "Will return a Lightweight Data Object and a set of HTTP GET endpoints that will return a list of those LDO's.\n" +
            "Or a singleton LDO for primary keys defined for TNAME. It uses the GetListOfOtherKeys to return the LIST of LDO's \n" +
            "So the resulting output might carry a LOT of code depending on the tables definition in the database.")]
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

            result += GetGetterWebMethodFromTableName(CN, TNAME, "cls" + TNAME, tpk.PKName, true, true);

            foreach(OtherTableKeys otk in otks)
            {
               result += GetGetterWebMethodFromTableName(CN, TNAME, "cls" +TNAME, otk.PKName, false, false);
            }

            return result;
        }

        [HttpGet]
        [Route("GetPOSTMethodForTable")]
        [SwaggerOperation(Summary =
            "Will return Code that generates a full database abstraction class for given TNAME (GENERATEDBTABLEMODEL=TRUE) as well.\n" +
            "LDO for the table (GENERATEINTERFACECLASS = True). It will then generate a HTTP POST endpoint that will take \n" +
            "From the BODY of the post one of those LDO's and leverage the database abstraction class to hydrate all the fields and call UPDATE()\n" +
            "To write out the record or update the record if there is already one in the target database, Update() interprets the Primary key field\n" +
            "to determine if an Insert or an Update gets called ")]
        public string GetPOSTMethodForTable(string CN = "DBwSSPI_Login", string TNAME = "SomeTable", Boolean GenerateInterfaceClass = true,Boolean GenerateDBTableModel = true)
        {
            string result = "";

            string TheTabs = "";

            //Cast it to a LIST because its just easier to work with that rater than a crappy Ienumerable
            List<Models.Field> TheFields = (List<Models.Field>)GetTableSchemaFields(CN, TNAME);

            if (GenerateInterfaceClass)
            {
                result += "\n//-----------------------------------------------------------------\n";
                result += "//--------  Interface Class                                   -----\n";
                result += "//-----------------------------------------------------------------\n\n";

                result += GetInterfaceClassFromSpecificTableName(CN, TNAME, TNAME) + "\n";
            }


            if (GenerateDBTableModel)
            {

                result += "\n//-----------------------------------------------------------------\n";
                result += "//--------  Database Class                                    -----\n";
                result += "//-----------------------------------------------------------------\n\n";

                result += GetTableModelForPost(CN, TNAME, "cls" + TNAME);

            }
            result += "\n//-----------------------------------------------------------------\n";
            result += "//--------  POST METHOD                                       -----\n";
            result += "//-----------------------------------------------------------------\n\n";

            result += "[HttpPost]\n";
            result += "[Route(\"Post" + TNAME +"\")]\n";
            result += "public void Post" + TNAME + "([FromBody] " + TNAME + " value )\n";
            result +="{\n\n";

            TheTabs = "\t";
            result += TheTabs + "string SQL = \"\";\n";
            result += TheTabs + "cls" + TNAME + 
                " Target = new cls" + TNAME + 
                "(\"" + FetchActualConnectionString(CN) + "\");\n";

            // Figure out what field is the ID field if any
            foreach (Models.Field theField in TheFields)
            {
                // Another Approach

                result += TheTabs + "Target." + theField.FieldName + " = value." + theField.FieldName + ";\n";

            }

            result += TheTabs + "target.Update();\n";

            result += "}\n\n";
            
            return result;
        }

        [HttpGet]
        [Route("GetWINFORMsDefinitionForTable")]
        [SwaggerOperation(Summary =
            "Will return a collection of code bits that can be inserted into\nA windows Forms application to hydrate a UI screen for the given Object")]
        public string GetWINFORMsDefinitionForTable(string CN = "DBwSSPI_Login", string TNAME = "MemberMain")
        {
            // use the call to retrieve the fields from the chosen table
            TheFields = (List<Models.Field>)GetTableSchemaFields(CN, TNAME);
            TableName = TNAME;
            
            
            string result = "";
            result += "// ======================================================================\n";
            result += "// ==This snippet replaces the designer generated content for the form ==\n";
            result += "// ======================================================================\n";
            result += "\n";

            result += GenerateWinFormsInitializeComponent() + "\n\n";

            result += "// ====================================================================================\n";
            result += "// ==This snippet would go into the Forms cs file directly supporting the above code ==\n";
            result += "// ====================================================================================\n";
            result += "\n";

            result += GeneratePacker() + "\n\n";
            result += GenerateUnPacker() + "\n\n";
            result += GenerateSupportRoutines() + "\n\n";
            result += GenerateButtonHandlers() + "\n\n";

            result += "// ==========================================================================================\n";
            result += "// ==This snippet is a defined form of the Interface Class Used in the PACK() and UNPACK() ==\n";
            result += "// ==========================================================================================\n";
            result += "\n";


            result += GetInterfaceClassFromSpecificTableNameInternal(CN, TNAME, TNAME,false);

            result += "\n";

            result += "// ===========================================================================================\n";
            result += "// ==This snippet is the LONG FORM class definition that implements INotifyProperty Changed ==\n";
            result += "// ==as well as Read/Write/Update/Delete functionality housed within the class itself       ==\n";
            result += "// ===========================================================================================\n";
            result += "\n";

            result += GetTableModel(CN, TNAME);

            return DoTheIndentation(result);
            
        }

        [HttpGet]
        [Route("GetHTMLFormDefinitionForTable")]
        [SwaggerOperation(Summary =
            "Will return a collection of CSS and HTML code to script up a form for the given database entity, Will use just HTML if DoJavaScriptStuff is false ")]
        public string GetHTMLFormDefinitionForTable(string CN = "DBwSSPI_Login", string TNAME = "MemberMain", bool DoJavaScriptStuff = true)
        {
            // use the call to retrieve the fields from the chosen table
            TheFields = (List<Models.Field>)GetTableSchemaFields(CN, TNAME);
            TableName = TNAME;
            
            
            string result = "";
            result += "// ======================================================================\n";
            result += "// == This snippet will be for the StyleSheet Referenced               ==\n";
            result += "// ======================================================================\n";
            result += "\n";

            result += GenerateCSSCode() + "\n\n";

            result += "// ====================================================================================\n";
            result += "// == This is the actual HTML Code generated                                         ==\n";
            result += "// ====================================================================================\n";
            result += "\n";

            result += GenerateHTMLCode(DoJavaScriptStuff) + "\n\n";


            return result;

        }

        // This method dynamically generates a XAML form definition by executing a SQL command and inspecting its result set schema.
        // The UI is represented in XAML and includes different controls (TextBox, CheckBox, DatePicker) for each field in the data schema.
        // It initializes basic Grid layout in XAML with two columns (first for field names, second for data entry controls).
        // For each field in the schema:
        //     - Appends a XAML TextBlock with field name to the first column of the grid.
        //     - Determines the .NET type equivalent of the field.
        //     - Based on the type of the field, it appends a corresponding XAML control to the second column in the Grid.
        //     - If the type doesn't match any of the specific cases, it defaults to creating a TextBox.
        //     - It increments a row index so the next control can go in the correct place.
        // The final XAML string is returned representing a UI form ready for WPF or UWP application, populates each UI control with corresponding field name.
        [HttpGet]
        [Route("GetXAMLFormDefinitionForSQLResult")]
        [SwaggerOperation(Summary =
            "Will return a Grid XAML Snippet for the given SQL Code Snippet. The SQL Snippet is subject to content length restrictions. Thus is not really usable for BIG stuff. The Resuting XAML will be a grid with the columns defined by the SQL Snippet. Easily copied and pasted into a container on a XAML usercontrol or window.")]
        public string GetXAMLFormDefinitionForSQLResult(string CN = "DBwSSPI_Login", string SQLCode = "Select top 1 * from SOMETABLE")
        {
            //string result = "";
            
            CN = FetchActualConnectionString(CN);
            
            List<Field> theFieldsList = (List<Field>)GetSchemaFieldsFromSQLCode(CN, SQLCode);

            StringBuilder xaml = new StringBuilder();
            xaml.AppendLine("<Grid>");
            xaml.AppendLine("    <Grid.ColumnDefinitions>");
            xaml.AppendLine("        <ColumnDefinition Width=\"Auto\"/>");
            xaml.AppendLine("        <ColumnDefinition Width=\"*\"/>");
            xaml.AppendLine("    </Grid.ColumnDefinitions>");
            xaml.AppendLine("    <Grid.RowDefinitions>");

            int rowIndex = 0;

            foreach (var item in theFieldsList)
            {
                xaml.AppendLine($"        <RowDefinition Height=\"Auto\"/>");
                rowIndex++;
            }
            
            xaml.AppendLine("    </Grid.RowDefinitions>");

            rowIndex = 0;

            foreach (var item in theFieldsList)
            {
                xaml.AppendLine($"    <TextBlock Text=\"{item.FieldName}\" Grid.Row=\"{rowIndex}\" Grid.Column=\"0\" VerticalAlignment=\"Center\" Margin=\"5\"/>");

                if (item.FieldType == typeof(bool).ToString())
                {
                    xaml.AppendLine($"    <CheckBox Grid.Row=\"{rowIndex}\" Grid.Column=\"1\" Name=\"chk{item.FieldName}\" Margin=\"5\"/>");
                }
                else if (item.FieldType == typeof(DateTime).ToString())
                {
                    xaml.AppendLine($"    <DatePicker Grid.Row=\"{rowIndex}\" Grid.Column=\"1\" Name=\"dtp{item.FieldName}\" Margin=\"5\"/>");
                }
                else if (item.FieldType == typeof(string).ToString())
                {
                    xaml.AppendLine($"    <TextBox Grid.Row=\"{rowIndex}\" Grid.Column=\"1\" Name=\"txt{item.FieldName}\" Margin=\"5\"/>");
                }
                else if (item.FieldType == typeof(Int128).ToString() ||
                         item.FieldType == typeof(Int64).ToString() ||
                         item.FieldType == typeof(Int32).ToString() ||
                         item.FieldType == typeof(Int16).ToString() ||
                         item.FieldType == typeof(UInt128).ToString() ||
                         item.FieldType == typeof(UInt64).ToString() ||
                         item.FieldType == typeof(UInt32).ToString() ||
                         item.FieldType == typeof(UInt16).ToString())
                {
                    xaml.AppendLine($"    <TextBox Grid.Row=\"{rowIndex}\" Grid.Column=\"1\" Name=\"int{item.FieldName}\" Margin=\"5\" Width=\"150\" HorizontalAlignment= \"Left\" />");
                }
                else if (item.FieldType == typeof(decimal).ToString() ||
                         item.FieldType == typeof(double).ToString() ||
                         item.FieldType == typeof(float).ToString())
                {
                    xaml.AppendLine($"    <TextBox Grid.Row=\"{rowIndex}\" Grid.Column=\"1\" Name=\"long{item.FieldName}\" Margin=\"5\" Width=\"200\" HorizontalAlignment= \"Left\" />");
                }
                else if (item.FieldType == typeof(byte[]).ToString())
                {
                    xaml.AppendLine($"    <TextBox Grid.Row=\"{rowIndex}\" Grid.Column=\"1\" Name=\"btyearray{item.FieldName}\" Margin=\"5\" Width=\"150\" HorizontalAlignment= \"Left\" />");
                }
                else
                {
                    xaml.AppendLine($"    <TextBox Grid.Row=\"{rowIndex}\" Grid.Column=\"1\" Name=\"unknown{item.FieldName}\" Margin=\"5\" Width=\"300\" HorizontalAlignment= \"Left\" />");
                }
                // Add more cases for other data types as needed

                rowIndex++;
            }

            xaml.AppendLine("</Grid>");
            return xaml.ToString();
        }

        // This will wrap a call to the GetXAMLFormDefinitionForSQLResult() method in a XAML Window object.
        // The resulting XAML will be a Window with a Grid containing the columns defined by the SQL Snippet.
        // The Windows will be sized to the given width and height. and if checked will be wrapped in a ScrollViewer.
        // The resulting XAML can be easily copied and pasted your code
        [HttpGet]
        [Route("GetXAMLFormDefinitionForSQLResultInAWindow")]
        [SwaggerOperation(Summary =
            "Will return a Grid XAML Snippet for the given SQL Code Snippet Wrapped within a XAML Window object. The SQL Snippet is subject to content length restrictions. Thus is not really usable for BIG stuff. The Resuting XAML will be a grid with the columns defined by the SQL Snippet. Easily copied and pasted into a container on a XAML usercontrol or window.")]
        public string GetXAMLFormDefinitionForSQLResultInAWindow(string CN = "DBwSSPI_Login", string SQLCode = "Select top 1 * from SOMETABLE",
            string WindowTitle = "Window Title Goes Here", string WindowWidth = "800", string WindowHeight = "500",
            string WindowXAMLClassName = "clsXAMLWindow", string WindowXAMLNameSpace = "MyNameSpace", bool WrapInAScrollViewer = true)
        {
            string Preample = "";
            Preample += $"<Window xmlns=\"https://github.com/avaloniaui\"\n" +
                        $"        xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"\n" +
                        $"        xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"\n" +
                        $"        xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"\n" +
                        $"        mc:Ignorable=\"d\" d:DesignWidth=\"{WindowWidth}\" d:DesignHeight=\"{WindowHeight}\"\n" +
                        $"        x:Class=\"{WindowXAMLNameSpace}.{WindowXAMLClassName}\"\n" +
                        $"        Title=\"{WindowTitle}\">\n\n";
            
            string Postamble = "</Window>";
            
            if (WrapInAScrollViewer)
            {
                Preample += "<ScrollViewer>\n";
                Postamble = "</ScrollViewer>\n</Window>";
            }
            
            string xaml = GetXAMLFormDefinitionForSQLResult(CN, SQLCode);
            return Preample + xaml.ToString() + "\n" + Postamble;
        }


        /// <summary>
        /// Returns a formatted SQL stanza for a given UGLY SQL code snippet.
        /// This method is highly subject to content length restrictions and is not recommended for large SQL queries. </summary> <param name="UGLYSQL">The UGLY SQL code snippet to be formatted.</param> <returns>
        /// <returns> The formatted SQL stanza. </returns>
        [HttpGet]
        [Route("MakeSQLUglyPretty")]
        [SwaggerOperation(Summary =
                       "Will Return a pretty SQL stanza for a supplied UGLY SQL Code Snippet. This is highly subject to content length restrictions. Thus is not really usable for BIG stuff")]
        public string MakeSQLUglyPretty(string UGLYSQL)
        {
            string result = "";

            SQL_Formatter.Formatter formatter = new SQL_Formatter.Formatter();

            string opts = "LeadingCommas=False;LeadingJoins=True;RemoveComments=False";

            result = formatter.Format(UGLYSQL, opts);
           
            return result;
        }

        /// <summary>
        /// Converts a ugly SQL code snippet to an formatted Pretty SQL code snippet.
        /// The source SQL code snippet is passed in the body of the POST request.
        /// bypassing the content length restrictions of a GET request.
        /// </summary>
        /// <param name="longBaby">The ugly SQL code snippet to convert.</param>
        /// <returns>The converted formatted SQL code snippet.</returns>
        [HttpPost]
        [Route("MakeSQLUglyFromPostPretty")]
        [SwaggerOperation(Summary = "Will Return a pretty SQL stanza for a supplied UGLY SQL Code Snippet. As the LOng SQL is taken from the body in a POST its much less subject to content length restrictions. Thus is usable for BIG stuff")]
        public string MakeSQLUglyFromPostPretty([FromBody] AreallyLongString longBaby)
        {
            string result = "";

            SQL_Formatter.Formatter formatter = new SQL_Formatter.Formatter();

            string opts = "LeadingCommas=False;LeadingJoins=True;RemoveComments=False";

            result = formatter.Format(longBaby.TheString, opts);
           
            return result;
        }

        /// <summary>
        /// Get the SQL script that can recreate a table or view in the database.
        /// </summary>
        /// <param name="CN">The connection string for the database. Default value is "DBwSSPI_Login".</param>
        /// <param name="TNAME">The name of the table or view to create the script for. Default value is "MemberMain".</param>
        /// <param name="ScriptIndexes">Specify whether to include indexes in the script. Default value is true.</param>
        /// <returns>The SQL script as a string. If the table or view does not exist, it returns an error message.</returns>
        [HttpGet]
        [Route("GetCreateScript")]
        [SwaggerOperation(Summary =
            "Will Return a SQL Script that will recreate the table or a view in the database ")]
        public string GetCreateScript(string CN = "DBwSSPI_Login", string TNAME = "MemberMain", bool ScriptIndexes = true)
        {
            string result = "";
            
            CN = FetchActualConnectionString(CN);
            
            // Parse the connection string to retrieve the database name
            var builder = new SqlConnectionStringBuilder(CN);
            string databaseName = builder.InitialCatalog;

            // Set up a connection to the SQL Server using the connection string
            ServerConnection serverConnection = new ServerConnection(new Microsoft.Data.SqlClient.SqlConnection(CN));
            Server server = new Server(serverConnection);

            // Connect to the specified database
            Database database = server.Databases[databaseName];
            
            // Get the table by name
            Table table = database.Tables[TNAME];

            if (table == null)
            {
                // see if the supplied table name is actually a View
                View view = database.Views[TNAME];

                if (view == null)
                {
                    // The table or view does not exist
                    result = $"Table or View '{TNAME}' not found in database '{databaseName}'.";
                    return result;
                }
                
                ScriptingOptions voptions = new ScriptingOptions
                {
                    ScriptDrops = false,  // Set to true if you want to include DROP VIEW statements
                    IncludeHeaders = true,
                };

                StringBuilder vscript = new StringBuilder();
                StringCollection vscriptCollection = view.Script(voptions);

                foreach (string line in vscriptCollection)
                {
                    vscript.AppendLine(line);
                    vscript.AppendLine("GO");
                }

                return vscript.ToString();
            }

            // Generate the Create Table script
            ScriptingOptions options = new ScriptingOptions
            {
                ScriptDrops = false,  // Set to true if you want to include DROP TABLE statements
                IncludeHeaders = true,
                ClusteredIndexes = ScriptIndexes,  // Set to false if you don't want to include clustered indexes
                NonClusteredIndexes = ScriptIndexes,  // Set to false if you don't want to include non-clustered indexes
                
            };
            
            StringBuilder script = new StringBuilder();
            StringCollection scriptCollection = table.Script(options);

            foreach (string line in scriptCollection)
            {
                script.AppendLine(line);
            }

            return script.ToString();
            
            //return result;
        }

        /// <summary>
        /// Retrieves a series of code snippets that can be pasted into a Windows Forms definition for the UI CRUD.
        /// </summary>
        /// <param name="CN">The connection string for the database.</param>
        /// <param name="SQLCode">The SQL code that retrieves data from the database.</param>
        /// <param name="WindowTitle">The title of the window in the Windows Forms application.</param>
        /// <param name="WindowWidth">The width of the window in pixels.</param>
        /// <param name="WindowHeight">The height of the window in pixels.</param>
        /// <param name="WindowClassName">The class name of the window.</param>
        /// <param name="WindowNameSpace">The namespace of the window.</param>
        /// <param name="DBOClassName">The class name for the database object.</param>
        /// <returns>The generated code snippets for the Windows Forms application.</returns>
        [HttpGet]
        [Route("GetWinFormsStuff")]
        [SwaggerOperation(Summary =
            "Will generate a series of code snippets that can be pasted into a Windows Forms definition for the UI CRUD")]
        public string GetWinFormsStuff(string CN = "DBwSSPI_Login", string SQLCode = "Select top 1 * from SOMETABLE",
            string WindowTitle = "Window Title Goes Here", string WindowWidth = "800", string WindowHeight = "500",
            string WindowClassName = "MyWindowClassName", string WindowNameSpace = "MyWindowNameSpace", string DBOClassName = "MyDBOObject")
        {
            string result = "";

            TheFields = (List<Field>)GetSchemaFieldsFromSQLCode(CN, SQLCode);

            // here we will Coerse the fields a bit
            foreach (Field f in TheFields)
            {
                f.FieldNameConverted = f.FieldName;

                if (f.FieldType.ToUpper().Contains(".STRING"))
                    f.FieldType = "VARCHAR";
                
                if (f.FieldType.ToUpper().Contains(".INT"))
                    f.FieldType = "INT";
                
                if (f.FieldType.ToUpper().Contains(".DATE"))
                    f.FieldType = "DATETIME";
                
                if (f.FieldType.ToUpper().Contains(".BOOL"))
                    f.FieldType = "BOOL";
                
                if (f.FieldType.ToUpper().Contains(".DOUB"))
                    f.FieldType = "DOUBLE";
                
                if (f.FieldType.ToUpper().Contains(".FLOA"))
                    f.FieldType = "FLOAT";
            }
            
            result += "//\n";
            result += "// This bit of code goes where your models are... \n";
            result += "//\n\n";

            result += GetInterfaceClassFromSQLCode(CN, SQLCode, DBOClassName);

            TableName = DBOClassName;
            
            result += "//\n";
            result += "// These methods will go into the form code itself\n";
            result += "//\n\n";
            result += GenerateSupportRoutines();
            
            result += GeneratePacker();
            result += GenerateUnPacker();

            result += GenerateButtonHandlers();
            
            
            
            result += "//\n";
            result += "// Replace the InitializeComponent in the code behind with this \n";
            result += "//\n\n";

            result += GenerateWinFormsInitializeComponent();
            
            return result;
        }


        /// <summary> Takes a Delimited List of Method Names and Generates a set of Method Stubs for a Class </summary> 
        /// <param name="MethodNames">A Delimited List of Method Names</param> <returns> A set of Method Stubs for a Class </returns>
        [HttpPost]
        [Route("GetMethodsFromNamesSupplied")]
        [SwaggerOperation(Summary =
          "Will generate a series of code snippets stubs for each methodname in the supplied delimited list")]
        public string GetMethodsFromNamesSupplied([FromBody]string MethodNames = "Strings each on their own line")
        {
            string result = "";

            string[] methods = MethodNames.Split(',');

            foreach (string method in methods)
            {
                result += "public void " + method + "()\n";
                result += "{\n\n";
                result += "\n\n";
                result += "}\n\n";
            }

            return result;
        }


        #region Private Stuff

        /// <summary>
        /// Generates the table model class for a database table.
        /// </summary>
        /// <param name="CN">The connection string name.</param>
        /// <param name="TN">The table name.</param>
        /// <param name="CNAME">The class name.</param>
        /// <returns>The generated table model class.</returns>
        private string GetTableModelForPost(
            string CN = "DBwSSPI_Login",
            string TN = "MemberMain",
            string CNAME = "clsMemberMain")
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

                s += "public partial class " + CNAME + " : INotifyPropertyChanged\n" +
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

        /// <summary>
        /// Fetches an actual connection string based on the provided key.
        /// </summary>
        /// <param name="TheKey">The key used to look up the connection string.</param>
        /// <returns>
        /// The supplied key is first checked to see if its likely an actual connection string.
        /// If so itself is returned. Otherwise, the key is used to look up the actual connection
        /// in the appsettings.json file. If a match is found, the actual connection string is
        /// pulled from the appsettings.json file and returned. If no match is found, an empty
        /// string is returned.
        /// </returns>
        private string FetchActualConnectionString(string TheKey)
        {
            if (TheKey.Trim().ToUpper().StartsWith("SERVER"))
            {
                return TheKey;
            }
            
            if (TheKey.Trim().ToUpper().StartsWith("DSN"))
            {
                return TheKey;
            }
            
            if (TheKey.Trim().ToUpper().StartsWith("DATA SOURCE"))
            {
                return TheKey;
            }
            
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

        /// <summary>
        /// Performs indentation for a given code string.
        /// </summary>
        /// <param name="code">The code string to be indented.</param>
        /// <returns>The indented code string.</returns>
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

        /// <summary>
        /// Creates a string consisting of multiple tab characters for indentation.
        /// </summary>
        /// <param name="indent">The number of tab characters to generate.</param>
        /// <returns>A string consisting of the specified number of tab characters.</returns>
        private string INDENT(int indent)
        {
            string s = "";

            for (int t = 0; t < indent; t++)
            {
                s += "\t";
            }

            return s;

        }

        /// <summary>
        /// This method generates private methods for inserting or updating data into a database table.
        /// </summary>
        /// <returns>A string representing the C# code generated private methods.</returns>
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

        /// <summary>
        /// Inserts the specified number of tabs into a string.
        /// </summary>
        /// <param name="numtabs">The number of tabs to insert.</param>
        /// <returns>A string with the specified number of tabs.</returns>
        private string Tabify(int numtabs)
        {
            string result = "";

            for (int i= 0; i < numtabs; i++)
            {
                result += "\t";

            }

            return result;
        }

        /// <summary>
        /// Takes a string and an integer count as input and returns a string representation
        /// of the string with each line surrounded by quotes and concatenated by a plus sign.
        /// </summary>
        /// <param name="TheString">The string to be converted.</param>
        /// <param name="tcount">The integer count indicating the number of tabs to be added at the beginning of each line, except the first line.</param>
        /// <returns>
        /// A string representation of the input string with each line surrounded by quotes and concatenated by a plus sign.
        /// </returns>
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
        
        #region Winforms Stuff

        private string GenerateSupportRoutines()
        {
            string s = "";
            s += "private int GetAsInteger(string input)\n";
            s += "{\n";
            s += "int result = 0;\n";
            s += "bool diditwork = int.TryParse(input, out result);\n";
            s += "return result;\n";
            s += "}\n";
            s += "\n";
            s += "private long GetAsLong(string input)\n";
            s += "{\n";
            s += "long result = 0;\n";
            s += "bool diditwork = long.TryParse(input, out result);\n";
            s += "return result;\n";
            s += "}\n";
            s += "\n";
            s += "private double GetAsDouble(string input)\n";
            s += "{\n";
            s += "double result = 0;\n";
            s += "bool diditwork = double.TryParse(input, out result);\n";
            s += "return result;\n";
            s += "}\n";
            s += "\n";
            s += "private DateTime GetAsDateTime(string input)\n";
            s += "{\n";
            s += "DateTime result = Convert.ToDateTime(null);\n";
            s += "bool diditwork = DateTime.TryParse(input, out result);\n";
            s += "return result;\n";
            s += "}\n";
            s += "\n";

            return s;
        }

        private string GenerateUnPacker()
        {
            string s = "\n";

            s += "private void Unpack(" + TableName + " thing)\n";
            s += "{\n";

            foreach (Field f in TheFields)
            {
                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME")
                {
                    s += "txt" + f.FieldNameConverted + ".Text = thing." + f.FieldNameConverted + " + \"\";\n";
                }

                if (f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                   f.FieldType == "TINYINT" || f.FieldType == "BIGINT" ||
                   f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                   f.FieldType == "FLOAT")
                {
                    s += "txt" + f.FieldNameConverted + ".Text = thing." + f.FieldNameConverted + ".ToString() + \"\";\n";
                }


                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "if (thing." + f.FieldNameConverted + " != Convert.ToDateTime(null))\n";
                    s += "{\n";
                    s += "dtp" + f.FieldNameConverted + ".Value = thing." + f.FieldNameConverted + ";\n";
                    s += "}\n";
                    s += "else\n";
                    s += "{\n";
                    s += "dtp" + f.FieldNameConverted + ".Value = Convert.ToDateTime(null);\n";
                    s += "}\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "if (thing." + f.FieldNameConverted + ")\n";
                    s += "{\n";
                    s += "chk" + f.FieldNameConverted + ".Checked = true;\n";
                    s += "}\n";
                    s += "else\n";
                    s += "{\n";
                    s += "chk" + f.FieldNameConverted + ".Checked = false;\n";
                    s += "}\n";
                }
            }

            s += "}\n";

            return s;
        }

        private string GeneratePacker()
        {
            string s = "\n";

            s += "private " + TableName + " Pack()\n";
            s += "{\n";

            s += "" + TableName + " thing = new " + TableName + "();\n\n";

            foreach (Field f in TheFields)
            {
                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME")
                {
                    s += "thing." + f.FieldNameConverted + " = txt" + f.FieldNameConverted + ".Text + \"\";\n";
                }

                if (f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT")
                {
                    s += "thing." + f.FieldNameConverted + " = GetAsInteger(txt" + f.FieldNameConverted + ".Text);\n";
                }

                if (f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT")
                {
                    s += "thing." + f.FieldNameConverted + " = GetAsDouble(txt" + f.FieldNameConverted + ".Text);\n";
                }

                if (f.FieldType == "BIGINT")
                {
                    s += "thing." + f.FieldNameConverted + " = GetAsLong(txt" + f.FieldNameConverted + ".Text);\n";
                }


                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "thing." + f.FieldNameConverted + " = GetAsDateTime(dtp" + f.FieldNameConverted + ".Text);\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "if (chk" + f.FieldNameConverted + ".Checked)\n";
                    s += "{\n";
                    s += "thing." + f.FieldNameConverted + " = true;\n";
                    s += "}\n";
                    s += "else\n";
                    s += "{\n";
                    s += "thing." + f.FieldNameConverted + " = false;\n";
                    s += "}\n";
                }
            }

            s += "\n";
            s += "return thing;\n";

            s += "}\n";

            return s;
        }

        private string GenerateButtonHandlers()
        {
            string s = "";

            s += "private void btnEnableAll_Click(object sender, EventArgs e)\n";
            s += "{\n";

            foreach (Field f in TheFields)
            {

                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME" || f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                    f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT")
                {
                    s += "this.txt" + f.FieldNameConverted + ".Enabled = true;\n";

                }

                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "this.dtp" + f.FieldNameConverted + ".Enabled = true;\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "this.chk" + f.FieldNameConverted + ".Enabled = true;\n";
                }

            }

            s += "}\n\n";


            s += "private void btnDisableAll_Click(object sender, EventArgs e)\n";
            s += "{\n";

            foreach (Field f in TheFields)
            {

                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME" || f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                    f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT")
                {
                    s += "this.txt" + f.FieldNameConverted + ".Enabled = false;\n";

                }

                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "this.dtp" + f.FieldNameConverted + ".Enabled = false;\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "this.chk" + f.FieldNameConverted + ".Enabled = false;\n";
                }

            }

            s += "}\n\n";

            s += "private void btnHideAll_Click(object sender, EventArgs e)\n";
            s += "{\n";

            foreach (Field f in TheFields)
            {

                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME" || f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                    f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT")
                {
                    s += "this.txt" + f.FieldNameConverted + ".Visible = false;\n";

                }

                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "this.dtp" + f.FieldNameConverted + ".Visible = false;\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "this.chk" + f.FieldNameConverted + ".Visible = false;\n";
                }

                s += "this.lbl" + f.FieldNameConverted + ".Visible = false;\n";

            }

            s += "}\n\n";

            s += "private void btnShowAll_Click(object sender, EventArgs e)\n";
            s += "{\n";

            foreach (Field f in TheFields)
            {

                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME" || f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                    f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT")
                {
                    s += "this.txt" + f.FieldNameConverted + ".Visible = true;\n";

                }

                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "this.dtp" + f.FieldNameConverted + ".Visible = true;\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "this.chk" + f.FieldNameConverted + ".Visible = true;\n";
                }

                s += "this.lbl" + f.FieldNameConverted + ".Visible = true;\n";

            }

            s += "}\n\n";

            return s;
        }

        private string GenerateWinFormsInitializeComponent()
        {

            string s = "";

            s += "#region Windows Form Designer generated code\n\n";

            s += "private void InitializeComponent()\n";
            s += "{\n";
            s += "this.components = new System.ComponentModel.Container();\n";

            foreach (Field f in TheFields)
            {
                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME" || f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                    f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT")
                {
                    s += "this.txt" + f.FieldNameConverted + "  = new System.Windows.Forms.TextBox();\n";
                }

                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "this.dtp" + f.FieldNameConverted + "  = new System.Windows.Forms.DateTimePicker();\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "this.chk" + f.FieldNameConverted + "  = new System.Windows.Forms.CheckBox();\n";
                }
            }

            foreach (Field f in TheFields)
            {
                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME" || f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                    f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT")
                {
                    s += "this.lbl" + f.FieldNameConverted + "  = new System.Windows.Forms.Label();\n";
                }

                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "this.lbl" + f.FieldNameConverted + "  = new System.Windows.Forms.Label();\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "this.lbl" + f.FieldNameConverted + "  = new System.Windows.Forms.Label();\n";
                }
            }

            s += "this.btnHideAll = new System.Windows.Forms.Button();\n";
            s += "this.btnShowAll = new System.Windows.Forms.Button();\n";
            s += "this.btnDisableAll = new System.Windows.Forms.Button();\n";
            s += "this.btnEnableAll = new System.Windows.Forms.Button();\n";

            s += "this.SuspendLayout();\n";

            int ctlnum = 0;
            int colnum = 0;
            int CTRLnum = 0;
            int ctrlX = 0;


            foreach (Field f in TheFields)
            {

                colnum = ctlnum / (int)20;

                ctrlX = (colnum * 320) + 160;


                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME" || f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                    f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT")
                {
                    s += "//\n";
                    s += "// txt" + f.FieldNameConverted + "\n";
                    s += "//\n";

                    s += "this.txt" + f.FieldNameConverted + ".Location = new System.Drawing.Point(" + ctrlX + ", " + ((CTRLnum * 36) + 12).ToString() + ");\n";
                    s += "this.txt" + f.FieldNameConverted + ".Margin = new System.Windows.Forms.Padding(4);\n";
                    s += "this.txt" + f.FieldNameConverted + ".Name = \"txt" + f.FieldNameConverted + "\";\n";
                    s += "this.txt" + f.FieldNameConverted + ".ReadOnly = false;\n";
                    s += "this.txt" + f.FieldNameConverted + ".Size = new System.Drawing.Size(160, 22);\n";
                    s += "this.txt" + f.FieldNameConverted + ".TabIndex = " + ctlnum.ToString() + ";\n";

                }

                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "//\n";
                    s += "// dtp" + f.FieldNameConverted + "\n";
                    s += "//\n";

                    s += "this.dtp" + f.FieldNameConverted + ".Format = System.Windows.Forms.DateTimePickerFormat.Short;\n";


                    s += "this.dtp" + f.FieldNameConverted + ".Location = new System.Drawing.Point(" + ctrlX + ", " + ((CTRLnum * 36) + 12).ToString() + ");\n";
                    s += "this.dtp" + f.FieldNameConverted + ".Name = \"dtp" + f.FieldNameConverted + "\";\n";
                    s += "this.dtp" + f.FieldNameConverted + ".Size = new System.Drawing.Size(120, 22);\n";
                    s += "this.dtp" + f.FieldNameConverted + ".TabIndex = " + ctlnum.ToString() + ";\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "//\n";
                    s += "// chk" + f.FieldNameConverted + "\n";
                    s += "//\n";

                    s += "this.chk" + f.FieldNameConverted + ".Location = new System.Drawing.Point(" + ctrlX + ", " + ((CTRLnum * 36) + 12).ToString() + ");\n";
                    s += "this.chk" + f.FieldNameConverted + ".AutoSize = true;\n";
                    s += "this.chk" + f.FieldNameConverted + ".Name = \"chk" + f.FieldNameConverted + "\";\n";
                    s += "this.chk" + f.FieldNameConverted + ".Text = \"" + f.FieldNameConverted + "\";\n";
                    s += "this.chk" + f.FieldNameConverted + ".Size = new System.Drawing.Size(100, 22);\n";
                    s += "this.chk" + f.FieldNameConverted + ".TabIndex = " + ctlnum.ToString() + ";\n";
                    s += "this.chk" + f.FieldNameConverted + ".UseVisualStyleBackColor = true;\n";

                }

                ctlnum += 1;
                CTRLnum += 1;

                if (CTRLnum == 20)
                {
                    CTRLnum = 0;
                }
            }



            ctlnum = 0;
            colnum = 0;
            CTRLnum = 0;
            ctrlX = 0;

            foreach (Field f in TheFields)
            {
                colnum = ctlnum / (int)20;

                ctrlX = (colnum * 320) + 16;

                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME" || f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                    f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT" || f.FieldType == "BOOL" || f.FieldType == "BIT" ||
                    f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" ||
                    f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "//\n";
                    s += "// lbl" + f.FieldNameConverted + "\n";
                    s += "//\n";

                    s += "this.lbl" + f.FieldNameConverted + ".Location = new System.Drawing.Point(" + ctrlX + ", " + ((CTRLnum * 36) + 12).ToString() + ");\n";
                    s += "this.lbl" + f.FieldNameConverted + ".Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);\n";
                    s += "this.lbl" + f.FieldNameConverted + ".Name = \"lbl" + f.FieldNameConverted + "\";\n";
                    s += "this.lbl" + f.FieldNameConverted + ".AutoSize = true;\n";
                    s += "this.lbl" + f.FieldNameConverted + ".Size = new System.Drawing.Size(160, 20);\n";
                    s += "this.lbl" + f.FieldNameConverted + ".TabIndex = " + ctlnum.ToString() + ";\n";
                    s += "this.lbl" + f.FieldNameConverted + ".Text = \"" + f.FieldNameConverted + "\";\n";
                }


                ctlnum += 1;
                CTRLnum += 1;

                if (CTRLnum == 20)
                {
                    CTRLnum = 0;
                }
            }

            // Add the 4 Buttons At the Bottom of the Interface

            var thestring = "";
            thestring += "// \n";
            thestring += "// btnHideAll\n";
            thestring += "// \n";
            thestring += "this.btnHideAll.Location = new System.Drawing.Point(14, 770);\n";
            thestring += "this.btnHideAll.Name = \"btnHideAll\";\n";
            thestring += "this.btnHideAll.Size = new System.Drawing.Size(111, 27);\n";
            thestring += "this.btnHideAll.TabIndex = 61;\n";
            thestring += "this.btnHideAll.Text = \"Hide All\";\n";
            thestring += "this.btnHideAll.UseVisualStyleBackColor = true;\n";
            thestring += "// \n";
            thestring += "// btnShowAll\n";
            thestring += "// \n";
            thestring += "this.btnShowAll.Location = new System.Drawing.Point(131, 770);\n";
            thestring += "this.btnShowAll.Name = \"btnShowAll\";\n";
            thestring += "this.btnShowAll.Size = new System.Drawing.Size(111, 27);\n";
            thestring += "this.btnShowAll.TabIndex = 62;\n";
            thestring += "this.btnShowAll.Text = \"Show All\";\n";
            thestring += "this.btnShowAll.UseVisualStyleBackColor = true;\n";
            thestring += "// \n";
            thestring += "// btnDisableAll\n";
            thestring += "// \n";
            thestring += "this.btnDisableAll.Location = new System.Drawing.Point(248, 770);\n";
            thestring += "this.btnDisableAll.Name = \"btnDisableAll\";\n";
            thestring += "this.btnDisableAll.Size = new System.Drawing.Size(111, 27);\n";
            thestring += "this.btnDisableAll.TabIndex = 63;\n";
            thestring += "this.btnDisableAll.Text = \"Disable All\";\n";
            thestring += "this.btnDisableAll.UseVisualStyleBackColor = true;\n";
            thestring += "// \n";
            thestring += "// btnEnableAll\n";
            thestring += "// \n";
            thestring += "this.btnEnableAll.Location = new System.Drawing.Point(365, 770);\n";
            thestring += "this.btnEnableAll.Name = \"btnEnableAll\";\n";
            thestring += "this.btnEnableAll.Size = new System.Drawing.Size(111, 27);\n";
            thestring += "this.btnEnableAll.TabIndex = 64;\n";
            thestring += "this.btnEnableAll.Text = \"Enable All\";\n";
            thestring += "this.btnEnableAll.UseVisualStyleBackColor = true;\n\n";

            s += thestring;


            s += "this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);\n";
            s += "this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;\n";
            s += "this.ClientSize = new System.Drawing.Size(1339, 821);\n\n";


            s += "this.Controls.Add(this.btnEnableAll);\n";
            s += "this.Controls.Add(this.btnDisableAll);\n";
            s += "this.Controls.Add(this.btnShowAll);\n";
            s += "this.Controls.Add(this.btnHideAll);\n";

            foreach (Field f in TheFields)
            {
                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME" || f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                    f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT")
                {
                    s += "this.Controls.Add(this.txt" + f.FieldNameConverted + ");\n";
                }

                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "this.Controls.Add(this.dtp" + f.FieldNameConverted + ");\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "this.Controls.Add(this.chk" + f.FieldNameConverted + ");\n";
                }

            }

            foreach (Field f in TheFields)
            {
                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME" || f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                    f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT")
                {
                    s += "this.Controls.Add(this.lbl" + f.FieldNameConverted + ");\n";
                }

                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "this.Controls.Add(this.lbl" + f.FieldNameConverted + ");\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "this.Controls.Add(this.lbl" + f.FieldNameConverted + ");\n";
                }

            }

            s += "this.Margin = new System.Windows.Forms.Padding(4);\n";
            s += "this.Name = \"frm" + TableName + "UI\";\n";
            s += "this.Text = \"frm" + TableName + "UI\";\n";
            s += "this.ResumeLayout(false);\n";
            s += "this.PerformLayout();\n";

            s += "}\n";

            s += "#endregion \n\n";

            foreach (Field f in TheFields)
            {
                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME" || f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                    f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT")
                {
                    s += "private System.Windows.Forms.TextBox txt" + f.FieldNameConverted + ";\n";
                }

                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "private System.Windows.Forms.DateTimePicker dtp" + f.FieldNameConverted + ";\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "private System.Windows.Forms.CheckBox chk" + f.FieldNameConverted + ";\n";
                }

            }

            foreach (Field f in TheFields)
            {
                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME" || f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                    f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                    f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                    f.FieldType == "FLOAT")
                {
                    s += "private System.Windows.Forms.Label lbl" + f.FieldNameConverted + ";\n";
                }

                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    s += "private System.Windows.Forms.Label lbl" + f.FieldNameConverted + ";\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {
                    s += "private System.Windows.Forms.Label lbl" + f.FieldNameConverted + ";\n";
                }

            }

            s += "private System.Windows.Forms.Button btnHideAll;\n";
            s += "private System.Windows.Forms.Button btnShowAll;\n";
            s += "private System.Windows.Forms.Button btnDisableAll;\n";
            s += "private System.Windows.Forms.Button btnEnableAll;\n";

            return s;
        }

        #endregion

        #region HTML and CSS Code Stuff

         private string GenerateHTMLCode(bool JS)
        {
            string s = "";

            string anon = "";

            if (JS )
            {
                // Header that references the JAVASCRIPT UI and JQUERY addons

                s = "<!DOCTYPE html>\n" +
               "<html lang=\"en\">\n" +
               "<head>\n" +
               "\t<meta charset=\"utf-8\">\n" +
               "\t<title>title</title>\n" +
               "\t<link rel=\"stylesheet\" href=\"JS\\jquery-ui.min.css\">\n" +
               "\t<link rel=\"stylesheet\" href=\"style.css\">\n" +
               "\t<script language=\"JavaScript\" src=\"JS\\jquery-1.12.0.min.js\" ></script>\n" +
               "\t<script language=\"JavaScript\" src=\"JS\\jquery-ui.min.js\" ></script>\n" +
               "\t<script language=\"JavaScript\">\n"+
               "REPLACETHISWITHANONYMOUSFUNCTION\n" +
               "\t</script>\n" +
               "</head>\n" +
               "<body>\n";
            }
            else
            {
                // Generic Header for the HTML Output

                s = "<!DOCTYPE html>\n" +
                "<html lang=\"en\">\n" +
                "<head>\n" +
                "\t<meta charset=\"utf-8\">\n" +
                "\t<title>title</title>\n" +
                "\t<link rel=\"stylesheet\" href=\"style.css\">\n" +

                "</head>\n" +
                "<body>\n";
            }

            

            s += "\t<div class=\"panel-frame\">\n";

            foreach (Field f in TheFields)
            {

                s += "\t\t<div class=\"panel-container\" id=\"container_" + f.FieldName + "\" >\n";
                
                if (f.FieldType == "VARCHAR" || f.FieldType == "CHAR" || f.FieldType == "NVARCHAR" ||
                    f.FieldType == "TEXT" || f.FieldType == "UNIQUEIDENTIFIER" || f.FieldType == "GUID" ||
                    f.FieldType == "SYSNAME")
                {

                    if (f.MaxLength > 200 || f.MaxLength < 0)
                    {
                        // encode this as a large multiline field

                        s += "\t\t\t<nav class=\"panel-left\">\n";
                        s += "\t\t\t\t" + f.FieldName + "\n";
                        s += "\t\t\t</nav>\n\n";

                        s += "\t\t\t<div class=\"panel-splitter\">\n";
                        s += "\t\t\t\t | \n";
                        s += "\t\t\t</div>\n\n";

                        s += "\t\t\t<div class=\"panel-right\">\n";
                        s += "\t\t\t\t<textarea rows=\"4\" cols=\"50\" name=\"fld_" + f.FieldName + "\" id=\"fld_" + f.FieldName + "\"> \n";
                        s += "\t\t\t\t</textarea>\n";
                        s += "\t\t\t</div>\n\n";
                    }
                    else
                    {
                    //    if (f.CROSSWALKTABLE != "")
                    //    {
                    //        // we are coding a crosswalked Item

                    //        s += "\t\t\t<nav class=\"panel-left\">\n";
                    //        s += "\t\t\t\t" + f.FieldName + "\n";
                    //        s += "\t\t\t</nav>\n\n";

                    //        s += "\t\t\t<div class=\"panel-splitter\">\n";
                    //        s += "\t\t\t\t | \n";
                    //        s += "\t\t\t</div>\n\n";

                    //        s += "\t\t\t<div class=\"panel-right\">\n";
                    //        s += "\t\t\t\t<input type=\"text\" list=\"data_list_" + f.FieldName + "\" + name=\"fld_" + f.FieldName + "\" id=\"fld_" + f.FieldName + "\" /> \n";
                    //        s += "\t\t\t</div>\n\n";

                    //        // Simple dataList

                    //        SqlConnection cn = new SqlConnection(DSN);
                    //        cn.Open();
                    //        string sql = "SELECT DISTINCT " + f.CROSSWALKVALUE + "," + f.CROSSWALKDISPLAY + " from " + f.CROSSWALKTABLE + " ORDER BY " + f.CROSSWALKDISPLAY;

                    //        SqlCommand cmd = new SqlCommand(sql, cn);

                    //        SqlDataReader r = cmd.ExecuteReader();

                    //        s += "\t\t\t<datalist id=\"data_list_" + f.FieldName + "\">\n";

                    //        while (r.Read())
                    //        {
                    //            s += "\t\t\t\t<option value=\"" + r[0] + "\" >" + r[1] + "</option>\n";
                    //        }
                    //        r.Close();
                    //        cmd.Dispose();
                    //        cn.Close();
                    //        cn.Dispose();

                    //        s += "\t\t\t</datalist>\n\n";
                            
                    //    }
                    //    else
                    //    {

                    //        s += "\t\t\t<nav class=\"panel-left\">\n";
                    //        s += "\t\t\t\t" + f.FieldName + "\n";
                    //        s += "\t\t\t</nav>\n\n";

                    //        s += "\t\t\t<div class=\"panel-splitter\">\n";
                    //        s += "\t\t\t\t | \n";
                    //        s += "\t\t\t</div>\n\n";

                    //        s += "\t\t\t<div class=\"panel-right\">\n";
                    //        s += "\t\t\t\t<input type=\"text\" maxlength=\"" + f.MaxLength.ToString() + "\" name=\"fld_" + f.FieldName + "\" id=\"fld_" + f.FieldName + "\" /> \n";
                    //        s += "\t\t\t</div>\n\n";
                    //    }
                    }                  
                    ////s += "this.txt" + f.FieldNameConverted + "  = new System.Windows.Forms.TextBox();\n";
                }

                if (f.FieldType == "INT" || f.FieldType == "SMALLINT" ||
                   f.FieldType == "TINYINT" || f.FieldType == "BIGINT" || f.FieldType == "DECIMAL" ||
                   f.FieldType == "DOUBLE" || f.FieldType == "MONEY" || f.FieldType == "CURRENCY" ||
                   f.FieldType == "FLOAT")
                {
                    s += "\t\t\t<nav class=\"panel-left\">\n";
                    s += "\t\t\t\t" + f.FieldName + "\n";
                    s += "\t\t\t</nav>\n\n";

                    s += "\t\t\t<div class=\"panel-splitter\">\n";
                    s += "\t\t\t\t | \n";
                    s += "\t\t\t</div>\n\n";

                    s += "\t\t\t<div class=\"panel-right\">\n";
                    s += "\t\t\t\t<input type=\"number\" name=\"fld_" + f.FieldName + "\" id=\"fld_" + f.FieldName + "\" /> \n";
                    s += "\t\t\t</div>\n\n";


                    //s += "this.txt" + f.FieldNameConverted + "  = new System.Windows.Forms.TextBox();\n";
                }


                if (f.FieldType == "DATETIME" || f.FieldType == "DATE" || f.FieldType == "DATETIME2" || f.FieldType == "SMALLDATE" || f.FieldType == "SMALLDATETIME")
                {
                    if (JS)
                    {
                        // Doing JQUERY UI Date Pickers

                        // is the anon variable empty? if so initialize it

                        if (anon == "")
                        {
                            anon = "\t\t$(document).ready(function() {\n";
                        }

                        s += "\t\t\t<nav class=\"panel-left\">\n";
                        s += "\t\t\t\t" + f.FieldName + "\n";
                        s += "\t\t\t</nav>\n\n";

                        s += "\t\t\t<div class=\"panel-splitter\">\n";
                        s += "\t\t\t\t | \n";
                        s += "\t\t\t</div>\n\n";

                        s += "\t\t\t<div class=\"panel-right\">\n";
                        s += "\t\t\t\t<input type=\"text\" name=\"fld_" + f.FieldName + "\" id=\"fld_" + f.FieldName + "\" /> \n";
                        s += "\t\t\t</div>\n\n";

                        anon += "\t\t\t$( \"#fld_" + f.FieldName + "\" ).datepicker();\n";
                    }
                    else
                    {
                        // Doing generic Date pickers

                        s += "\t\t\t<nav class=\"panel-left\">\n";
                        s += "\t\t\t\t" + f.FieldName + "\n";
                        s += "\t\t\t</nav>\n\n";

                        s += "\t\t\t<div class=\"panel-splitter\">\n";
                        s += "\t\t\t\t | \n";
                        s += "\t\t\t</div>\n\n";

                        s += "\t\t\t<div class=\"panel-right\">\n";
                        s += "\t\t\t\t<input type=\"date\" name=\"fld_" + f.FieldName + "\" id=\"fld_" + f.FieldName + "\" /> \n";
                        s += "\t\t\t</div>\n\n";
                    }
                    
                    //s += "this.dtp" + f.FieldNameConverted + "  = new System.Windows.Forms.DateTimePicker();\n";
                }

                if (f.FieldType == "BOOL" || f.FieldType == "BIT")
                {

                    s += "\t\t\t<nav class=\"panel-left\">\n";
                    s += "\t\t\t\t" + f.FieldName + "\n";
                    s += "\t\t\t</nav>\n\n";

                    s += "\t\t\t<div class=\"panel-splitter\">\n";
                    s += "\t\t\t\t | \n";
                    s += "\t\t\t</div>\n\n";

                    s += "\t\t\t<div class=\"panel-right\">\n";
                    s += "\t\t\t\t<input type=\"checkbox\" name=\"fld_" + f.FieldName + "\" id=\"fld_" + f.FieldName + "\" /> \n";
                    s += "\t\t\t</div>\n\n";

                    //s += "this.chk" + f.FieldNameConverted + "  = new System.Windows.Forms.CheckBox();\n";
                }

                s += "\t\t</div>\n";

            }

            s += "\t</div>\n" +
                  "</body>\n" +
                  "</html>\n";

            if (JS)
            {
                // do some cleanup and replace the ANON placeholder with the contents of the ANON variable

                anon += "\t\t});\n";

                s = s.Replace("REPLACETHISWITHANONYMOUSFUNCTION", anon);
            }

            return s;
        }

        private string GenerateCSSCode()
        {
            string s = "";

            s = ".panel-container {\n" +
                "\tdisplay: flex;\n" +
                "\tflex-direction: row;\n" +
                "\tjustify-content: space-around;\n" +
                "\tflex-wrap: nowrap;\n" +
                "\talign-items: stretch;\n" +
                "}\n\n" +

                ".panel-left {\n" +
                "\tflex: none;\n" +
                "\twidth: 200px;\n" +
                "}\n\n" +

                ".panel-splitter {\n" +
                "\tflex: none;\n" +
                "\twidth: 20px;\n" +
                "}\n\n" +

                ".panel-right {\n" +
                "\tflex: 1;\n" +
                "}\n\n";

            return s;
        }

        
        #endregion
        
        #region InterfaceClasses



        #endregion
    }

    /// <summary>
    /// Will be used to pass a list of tables and their primary keys in the API
    /// </summary>
    public class TablesAndPKs
    {
        public string TableName { get; set; } = "";
        public string PKName { get; set; } = "";
    }

    /// <summary>
    /// Will be used to pass a list of tables and their other key fields keys in the API
    /// </summary>
    public class OtherTableKeys
    {
        public string TableName { get; set; } = "";
        public string PKName { get; set; } = "";
    }
    
    /// <summary>
    /// This is a class that is used to pass a string to the API
    /// </summary>
    public class AreallyLongString
    {
        public string TheString { get; set; } = "";
    }
    
    /// <summary>
    /// Unused ATM
    /// </summary>
    public class SqlQueryModel 
    {
        public string CN { get; set; }
        public string SQLCode { get; set; }
    }
    
    /// <summary>
    /// Used in connecting Posted JSON in the BODY of a request to Variables for consumption
    /// Does so Asynchronously
    /// </summary>
    public class PlainTextModelBinder : IModelBinder
    {
        
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            // Read the request body as plain text
            var reader = new StreamReader(bindingContext.HttpContext.Request.Body);
            var text = reader.ReadToEnd();

            // Set the result as a string
            bindingContext.Result = ModelBindingResult.Success(text);
            return Task.CompletedTask;
        }
    }
    
}

