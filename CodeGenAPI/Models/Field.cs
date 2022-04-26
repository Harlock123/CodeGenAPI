using System;
namespace CodeGenAPI.Models
{
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
        }

        public Field(string fname, string ftype, int maxlen, bool nulls, bool identity, int precision, int scale, bool crw, string crt, string crv, string crd)
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
        }

    }
}

