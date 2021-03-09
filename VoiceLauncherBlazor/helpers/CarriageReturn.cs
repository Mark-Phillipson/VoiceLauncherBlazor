using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceLauncherBlazor.helpers
{
    public static class CarriageReturn
    {
		public static string ReplaceForCarriageReturnChar(string strInput)

		{

			int tmpCounter = 0;

			string strTmp1 = "";

			string strTmp2 = "";



			tmpCounter = strInput.IndexOf("\\r");



			if (tmpCounter == -1)

			{

				return strInput;

			}

			else

			{

				strTmp1 = strInput.Substring(0, tmpCounter);

				strTmp2 = strInput.Substring(strTmp1.Length + 2, strInput.Length

	  - (tmpCounter + 2));

				strTmp1 = strTmp1 + (char)13 + strTmp2;

				return ReplaceForCarriageReturnChar(strTmp1);

			}
		}


	}
}
