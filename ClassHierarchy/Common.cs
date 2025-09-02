
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Text;


namespace ClassHierarchy
{
    public class Common
    {
        public enum BorderStyle { None, SolidBorder, SolidBorderSolidGridlines, BoldBorder, BoldBorderSolidGridlines, Summary }

        public enum TextJustification { Left, Center, Right }

        public enum FontWeight { Normal, Bold, BoldHeader }

    }


    public class Definitions
    {
        #region Enums for internal purposes
        /// <summary>Database server type.</summary>
        public enum DbServerType { None = 0, Oracle, SqlServer, Access, Excel }

        /// <summary>Database data type.</summary>
        public enum DbServerDataType { NONE, ECON, ADD, XL }

        /// <summary>Enumeration to represent the conditional and other operators.</summary>
        public enum Operator { EQ, GTE, GT, LTE, LT, NEQ, LIKE, IN }

        /// <summary>Enumeration to represent the Junction operators.</summary>
        public enum Junction { NONE, AND, OR, ON }

        /// <summary>Enumeration supporting two directions for sorting: ascending/descending.</summary>
        public enum SortDirection { Ascending, Descending }
        #endregion

        #region Enums for charting
        /// <summary>Chart display type.</summary>
        public enum ChartType { Line, Scatter, ScatterLine }

        /// <summary>Fit curve type for built in Excel charts.</summary>
        public enum ChartFitCurve { None, Linear, Poly2, Poly3, MovingAverage5 }

        /// <summary>Shape types for displaying a single point.</summary>
        public enum ChartPointType { Circle, Square, Triangle, Diamond, CircleFilled, SquareFilled, TriangleFilled, DiamondFilled, Dot, X, Asterisk, Plus, None }

        /// <summary>Border shape types for tagging indication.</summary>
        public enum ChartPointBorderType { None = 0, Circle = 1, Square = 2, Triangle = 3 }
        #endregion

        #region Status colors
        public static Color VALID_COLOR { get { return Color.LightGreen; } }
        public static Color INVALID_COLOR { get { return Color.Pink; } }
        public static Color WARN_COLOR { get { return Color.Plum; } }
        public static Color NEUTRAL_COLOR { get { return SystemColors.Control; } }
        #endregion

        #region Const strings and numerical values
        /// <summary>This string should be used to initialize elements that should always be initialized and have a valid value.</summary>
        public const string UNKNOWN_STRING = "???";

        /// <summary>Maximum size for comments.</summary>
        public const int MAX_COMMENT_SIZE = 1000;

        /// <summary>Maximum size for filepath/name.</summary>
        public const int MAX_FILE_PATH = 255;

        /// <summary>Maximum sort order.</summary>
        public const int MAX_SORT_ORDER = 4;

        /// <summary>Maximum queries to retain for viewing.</summary>
        public const int MAX_RETAINED_QUERIES = 20;

        /// <summary>This many MRU allowed.</summary>
        public const int MAX_RECENT_VALS = 20;

        /// <summary>This many favorites allowed.</summary>
        public const int MAX_FAVORITES = 50;

        /// <summary>Indicates verified.</summary>
        public const string VERIFIED = ""; // "***VERIFIED***";

        /// <summary>Indicates unverified.</summary>
        public const string UNVERIFIED = "***UNVERIFIED***";

        /// <summary>Indicator for FLRs.</summary>
        public const string USER_COLUMN = "UC_";

        /// <summary>Grouping delimiters for parsing user entered values.</summary>
        public const string LIST_DELIM = ",";

        /// <summary>Grouping delimiters for parsing user entered values.</summary>
        public const string FIELD_DELIM = " ";
        #endregion

        #region Database specifiers
        /// <summary>Formatting of sql strings. IDAS continues to use the OARS views.
        public const string DB_PREFIX = "OARS2";

        /// <summary>Formatting of sql strings.</summary>
        public const string DB_PREFIX_U = DB_PREFIX + "_";

        /// <summary>Formatting of sql strings.</summary>
        public const string DB_PREFIX_ADD = "ADDUSER";

        /// <summary>Formatting of sql strings.</summary>
        public const string SQL_ESCAPE = "|";
        #endregion

        #region Format specifiers
        /// <summary>Formatting of date times.</summary>
        public const string TIME_FORMAT = "HH':'mm':'ss";

        /// <summary>Formatting of date times w/ milliseconds.</summary>
        public const string TIME_FORMAT_MSEC = "HH':'mm':'ss.fff";

        /// <summary>Formatting of date times.</summary>
        public const string DATE_FORMAT = "yyyy'-'MM'-'dd";

        /// <summary>Formatting of date times.</summary>
        public const string DATE_TIME_FORMAT = "yyyy'-'MM'-'dd HH':'mm':'ss";

        /// <summary>Formatting of date times. This is also what the database returns.</summary>
        public const string DATE_TIME_FORMAT_MSEC = "yyyy'-'MM'-'dd HH':'mm':'ss.fff";

        /// <summary>Format specifier for int values.</summary>
        public const string INT_FORMAT = "##################0";

        /// <summary>Format specifier for float and double values with two decimal places.</summary>
        public const string FLOAT_FORMAT_2 = "##################0.0##;-##################0.0##;0";

        /// <summary>Format specifier for float and double values with three decimal places.</summary>
        public const string FLOAT_FORMAT_3 = "##################0.0###;-##################0.0###;0";
        #endregion
    }
}
