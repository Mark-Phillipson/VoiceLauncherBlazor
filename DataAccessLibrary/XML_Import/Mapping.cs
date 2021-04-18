namespace DataAccessLibrary.XML_Import
{
    public static class Mapping
    {
        /// <summary>
        /// Value for Table Name
        /// </summary>
        public static int KnowbrainerCommandsTable { get; } = 0;
        /// <summary>
        /// Value for Table Name
        /// </summary>
        public static int CommandsTable { get; } = 1;
        /// <summary>
        /// Value for Table Name
        /// </summary>
        public static int CommandTable { get; } = 2;
        /// <summary>
        /// Value for Table Name
        /// </summary>
        public static int ContentTable { get; } = 3;
        /// <summary>
        /// Value for Table Name
        /// </summary>
        public static int ListsTable { get; } = 4;
        /// <summary>
        /// Value for Table Name
        /// </summary>
        public static int ListTable { get; } = 5;
        /// <summary>
        /// Value for Table Name
        /// </summary>
        public static int ValueTable { get; } = 6;
        /// <summary>
        /// Value for Table Name
        /// </summary>
        // Commands
        /// <summary>
        /// Commands Table Primary Key
        /// </summary>
        public static int PrimaryKey_Commands { get; } = 0;
        /// <summary>
        /// Commands Table Field Index Value
        /// </summary>
        public static int Scope { get; } = 1;
        /// <summary>
        /// Commands Table Field Index Value
        /// </summary>
        public static int Module { get; } = 2;
        /// <summary>
        /// Commands Table Field Index Value
        /// </summary>
        public static int Company { get; } = 3;
        /// <summary>
        /// Commands Table Field Index Value
        /// </summary>
        public static int ModuleDescription { get; } = 4;
        /// <summary>
        /// Commands Table Field Index Value
        /// </summary>
        public static int WindowTitle { get; } = 5;
        // Command
        /// <summary>
        /// Command Table Field Index Value
        /// </summary>
        public static int Description { get; } = 0;
        /// <summary>
        /// Command Table Field Index Value
        /// </summary>
        public static int PrimaryKey_Command { get; } = 1;
        /// <summary>
        /// Command Table Field Index Value
        /// </summary>
        public static int ScriptName { get; } = 2;
        /// <summary>
        /// Command Table Field Index Value
        /// </summary>
        public static int Group { get; } = 3;
        /// <summary>
        /// Command Table Field Index Value
        /// </summary>
        public static int Enabled { get; } = 4;
        /// <summary>
        /// Command Table Field Index Value
        /// </summary>
        public static int CommandsFK { get; } = 5;
        // Content
        /// <summary>
        /// Content Table Field Index Value
        /// </summary>
        public static int ScriptType { get; } = 0;
        /// <summary>
        /// Content Table Field Index Value
        /// </summary>
        public static int Content { get; } = 1;
        /// <summary>
        /// Content Table Field Index Value
        /// </summary>
        public static int CommandFK { get; } = 2;
        /// <summary>
        /// Content Table Field Index Value
        /// </summary>
        // List

        /// <summary>
        /// List Table Field Index Value
        /// </summary>
        public static int PrimaryKey { get; } = 0;
        /// <summary>
        /// List Table Field Index Value
        /// </summary>
        public static int Name { get; } = 1;
        /// <summary>
        /// List Table Field Index Value (always zero)
        /// </summary>
        public static int ListsFK { get; } = 2;
        // value
        /// <summary>
        /// value Table Field Index Value
        /// </summary>
        public static int Value { get; } = 0;
        /// <summary>
        /// value Table Field Index Value
        /// </summary>
        public static int ListFK { get; } = 1;








    }
}
