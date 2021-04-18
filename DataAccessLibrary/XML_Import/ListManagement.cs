using System.Collections.Generic;

namespace DataAccessLibrary.XML_Import
{
    public static class ListManagement
    {
        public static string BuildListFilter(string commandName, string filter)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return "";
            }
            List<string> lists = GetListsName(commandName);
            foreach (var list in lists)
            {
                filter = filter + (filter.Length > 0 ? " Or " : "") + "name = '" + list + "'";
            }
            return filter;
        }

        public static List<string> GetListsName(string commandName)
        {
            List<string> lists = new List<string>();
            var position1 = commandName.IndexOf("<");
            var position2 = commandName.IndexOf(">");
            var temporary = commandName;
            while (position2 > 0)
            {
                var listName = temporary.Substring(position1 + 1, position2 - position1 - 1);
                lists.Add(listName);
                if (temporary.Length > position2 + 2)
                {
                    temporary = temporary.Substring(position2 + 2);
                    position1 = temporary.IndexOf("<");
                    position2 = temporary.IndexOf(">");
                }
                else
                {
                    temporary = "";
                    position2 = 0;
                }
            }
            return lists;
        }

        public static bool HasLists(string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return false;
            }
            if (commandName.Contains("<") && commandName.Contains(">"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
