﻿using System;
namespace CodeGenAPI.Models
{
    /// <summary>
    /// The Field class is used to store the field information for a table
    /// used in the CodeGenAPI are source for most of its work
    /// </summary>
    public class Field
    {
        public bool AllowNulls { get; set; } = false;
        public string FieldName { get; set; } = "";
        public string FieldNameConverted { get; set; } = "";
        public string FieldType { get; set; } = "";
        public bool IsIdentity { get; set; } = false;
        public int MaxLength { get; set; } = 0;
        public int Precision { get; set; } = 0;
        public int Scale { get; set; } = 0;
        public bool CROSSWALK { get; set; } = false;
        public string CROSSWALKTABLE { get; set; } = "";
        public string CROSSWALKVALUE { get; set; } = "";
        public string CROSSWALKDISPLAY { get; set; } = "";
        public string TABLENAME { get; set; } = "";

        public Field()
        {

        }

        public Field(string fname, string ftype, int maxlen, bool nulls, bool identity, int precision, int scale)
        {
            FieldName = fname;
            FieldNameConverted = fname.Replace(" ", "_");
            FieldType = ftype;
            MaxLength = maxlen;
            AllowNulls = nulls;
            IsIdentity = identity;
            Precision = precision;
            Scale = scale;
            CROSSWALK = false;
            CROSSWALKTABLE = "";
            CROSSWALKVALUE = "";
            CROSSWALKDISPLAY = "";
            TABLENAME = "";
        }

        public Field(string fname, string ftype, int maxlen, bool nulls, bool identity, int precision, int scale, bool crw, string crt, string crv, string crd, string tname )
        {
            FieldName = fname;
            FieldNameConverted = fname.Replace(" ", "_");
            FieldType = ftype;
            MaxLength = maxlen;
            AllowNulls = nulls;
            IsIdentity = identity;
            Precision = precision;
            Scale = scale;
            CROSSWALK = crw;
            CROSSWALKTABLE = crt;
            CROSSWALKVALUE = crv;
            CROSSWALKDISPLAY = crd;
            TABLENAME = tname;
        }

    }
}

