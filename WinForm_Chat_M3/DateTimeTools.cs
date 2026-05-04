using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DateTimeTools
{
    public string GetDate()
    {
        return DateTime.Now.ToString("dddd, MMMM dd, yyyy", CultureInfo.GetCultureInfo("en-US"));
    }

    public string GetTime()
    {
        return DateTime.Now.ToString("HH:mm:ss");
    }
}
