using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LunarBirthICS.Pages
{
    public class IndexModel : PageModel
    {
        public string CustomScript { get; set; }
        public void OnGet()
        {

        }

        public static DateTime? SearchYear(int year, int searchMonth, int searchDay)
        {
            ChineseLunisolarCalendar lunaClaendar = new ChineseLunisolarCalendar();
            var dateStart = new DateTime(year, searchMonth, searchDay);
            var dateEnd = dateStart.AddDays(90);
            for (var target = dateStart; target <= dateEnd; target = target.AddDays(1))
            {

                if (lunaClaendar.GetMonth(target) == searchMonth && lunaClaendar.GetDayOfMonth(target) == searchDay)
                {
                    return target;
                }
            }
            return null;
        }

        public async Task<IActionResult> OnPostDownload(string title, string context, string txtDate2, string month, string day)
        {
            //   CustomScript = "<script>toastr.success('Success');</script>";

            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "icstmp");





            var source = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "ics2.txt");
            var source2 = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "ics1.txt");

            var str = "";
            var checkTag = 0;
            for (var i = DateTime.Now.Year; i <= 2100; i++)
            {
                try
                {
                    var tartgetD = SearchYear(i, int.Parse(month), int.Parse(day));

                    if (tartgetD.HasValue)
                    {

                        var s = source;
                        s = s.Replace("{ID}", tartgetD.Value.ToString("yyyyMMddTHHmm"));
                        //
                        s = s.Replace("{START}", tartgetD.Value.ToString("yyyyMMddT000000"));
                        s = s.Replace("{END}", tartgetD.Value.ToString("yyyyMMddT235900"));
                        s = s.Replace("{NOW}", DateTime.Now.ToString("yyyyMMddTHHmm00"));
                        s = s.Replace("{NAME}", title);
                        s = s.Replace("{DESC}", context);
                        str += s;
                        checkTag++;
                    }
                }
                catch {
                    continue;
                }
            }

            if (checkTag <= 0) {
                CustomScript = "<script>toastr.error('沒有資料可以製作，要不要檢查一下日期');</script>";
                return Page();
            }

            source2 = source2.Replace("{DATA}", str);


            var fileName = Guid.NewGuid().ToString("N");


            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "icstmp" + Path.DirectorySeparatorChar + fileName + ".ics", source2);





            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "icstmp" + Path.DirectorySeparatorChar + fileName + ".ics", FileMode.Open))
            {
                await stream.CopyToAsync(memoryStream);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            CustomScript = "<script> HoldOn.close();</script>";
            return new FileStreamResult(memoryStream, "text/calendar");

        }

    }
}
