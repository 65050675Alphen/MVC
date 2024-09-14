using CowStrike2_MVC_65050675.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CowStrike2_MVC_65050675.Controllers
{
    public class CowStrikeController : Controller
    {
        private readonly ILogger<CowStrikeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private string JsonFilePath => Path.Combine(_webHostEnvironment.WebRootPath, "CowDataSource", "CowData.json");

        public CowStrikeController(ILogger<CowStrikeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View("CowStrikeMain");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Search(string cowId)
        {
            // ตรวจสอบว่า cowId มีค่าอยู่หรือไม่
            if (!string.IsNullOrEmpty(cowId))
            {
                // ตรวจสอบให้แน่ใจว่ารหัสวัวมี 8 ตัวและตัวเลขนำหน้าไม่เป็น 0
                if (IsValidCowId(cowId))
                {
                    // ทำการค้นหาข้อมูลวัวที่มีรหัสตรงกับ cowId
                    // คุณสามารถดึงข้อมูลจากฐานข้อมูลหรือแหล่งข้อมูลอื่นๆ ที่นี่
                    // ตัวอย่างการคืนค่าผลลัพธ์ไปยัง view ที่ชื่อ "Details"
                    // โหลดข้อมูลจากไฟล์ JSON
                    var cows = LoadCowsFromJson();

                    // ค้นหาว่าวัวที่มีรหัสนี้มีอยู่ในชุดข้อมูลหรือไม่
                    var cowDetails = cows.FirstOrDefault(c => c.id == cowId);

                    if (cowDetails != null)
                    {
                        if (cowDetails.breed == "ขาว")
                        {
                            return View("WhiteCow", cowDetails);
                        }
                        else if (cowDetails.breed == "น้ำตาล")
                        {
                            return View("BrownCow", cowDetails);
                        }
                        
                    }
                    else
                    {
                        // ถ้ารหัสไม่ตรงกับข้อมูลในไฟล์ JSON
                        ViewBag.ErrorMessage = "ไม่พบข้อมูลวัวที่มีรหัสนี้";
                        return View("CowStrikeMain");
                    }
                    
                }
                else
                {
                    // ถ้ารหัสไม่ถูกต้อง ให้ส่งกลับไปยังหน้าหลักหรือแสดงข้อผิดพลาด
                    ViewBag.ErrorMessage = "รหัสวัวต้องมี 8 ตัวและเลขนำหน้าไม่เป็น 0";
                    return View("CowStrikeMain");
                }
            }

            // ถ้าไม่มีข้อมูลให้กลับไปที่หน้าหลัก
            return RedirectToAction("CowStrikeMain");
        }

        // ฟังก์ชันสำหรับตรวจสอบความถูกต้องของรหัสวัว
        private bool IsValidCowId(string cowId)
        {
            // ตรวจสอบให้แน่ใจว่ารหัสวัวมี 8 ตัวและตัวเลขนำหน้าไม่เป็น 0
            return Regex.IsMatch(cowId, @"^[1-9][0-9]{7}$");
        }

        private List<Cow> LoadCowsFromJson()
        {
            var jsonString = System.IO.File.ReadAllText(JsonFilePath);
            return JsonSerializer.Deserialize<List<Cow>>(jsonString);
        }

        [HttpPost]
        public IActionResult FeedLemon([FromBody] Request request)
        {
            var cows = LoadCowsFromJson();
            var cow = cows.FirstOrDefault(c => c.id == request.CowId);

            if (cow == null)
            {
                ViewBag.ErrorMessage = "ไม่พบข้อมูลวัวที่มีรหัสนี้";
                return Json(new { success = false, message = $"ไม่พบข้อมูลวัวที่มีรหัสนี้" });
            }

            // Set the cow as having eaten lemon
            cow.HasEatenLemon = true;
            SaveCowsToJson(cows);

            return Json(new { success = true, message = $"วัวที่มีรหัส {request.CowId} ได้รับการป้อนมะนาวแล้ว" });
        }
        [HttpPost]
        public JsonResult MilkAjax([FromBody] Request request)
        {
            if (string.IsNullOrEmpty(request.CowId))
            {
                return Json(new { success = false, message = "รหัสวัวไม่ถูกต้อง" });
            }

            var cows = LoadCowsFromJson();
            var cow = cows.FirstOrDefault(c => c.id == request.CowId);

            if (cow == null)
            {
                return Json(new { success = false, message = "ไม่พบข้อมูลวัวที่มีรหัสนี้" });
            }

            if(cow.Color == "ฟ้า")
            {
                return Json(new { success = false, message = "ไม่สามารถรีดนมได้เนื่องจากวัวเป็น BSOD" });
            }
            // Check for milk type, considering if the cow has eaten lemon
            string milkType = GetMilkType(cow);


            if (milkType == "Soy Milk" || milkType == "Almond Milk")
            {
                // หากเป็นนมถั่วเหลืองและวัวไม่ได้กินมะนาว จะกลายเป็น BSOD
                cow.IsBSOD = true;
                cow.Color = "ฟ้า"; // เปลี่ยนสีวัวเป็นฟ้า
                SaveCowsToJson(cows);

                return Json(new { success = false, message = $"วัวที่มีรหัส {request.CowId} ผลิตนมถั่วเหลืองและกลายเป็น BSOD (สีฟ้า).", milkType });
            }

            // ถ้าไม่เป็น BSOD, ดำเนินการรีดนมปกติ
            cow.BottleCount = (cow.BottleCount ?? 0) + 1;
            cow.HasEatenLemon = false; // รีเซ็ตสถานะการป้อนมะนาว
            SaveCowsToJson(cows);

            return Json(new { success = true, message = $"รีดนมสำเร็จสำหรับวัวที่มีรหัส {request.CowId}. ประเภทนม: {milkType}", milkType});
        }

        private string GetMilkType(Cow cow)
        {
            string milkType;
            var random = new Random();

            if (cow.breed.Equals("ขาว", StringComparison.OrdinalIgnoreCase))
            {
                // Check if cow has eaten lemon
                if (cow.HasEatenLemon)
                {
                    milkType = "Sour Milk"; // นมเปรี้ยวจากการป้อนมะนาว
                }
                else
                {
                    // Determine if soy milk is produced
                    var ageVariance = cow.age.months % 12;
                    var chance = 0.5 * ageVariance;
                    if (random.NextDouble() < chance / 100) // Convert chance to a decimal probability
                    {
                        milkType = "Soy Milk";
                    }
                    else
                    {
                        milkType = "Plain Milk"; // นมจืด
                    }
                }
            }
            else if (cow.breed.Equals("น้ำตาล", StringComparison.OrdinalIgnoreCase))
            {
                var ageVariance = cow.age.months % 12;
                var chance = 0.5 * ageVariance;
                if (random.NextDouble() < chance / 100) // Convert chance to a decimal probability
                {
                    milkType = "Almond Milk";
                }
                else
                {
                    milkType = "Chocolate Milk"; // นมช็อกโกแลต
                }
               
            }
            else
            {
                milkType = "Unknown Milk Type";
            }

            return milkType;
        }
        public void SaveCowsToJson(List<Cow> cows)
        {
            // แปลง List<Cow> เป็น JSON string
            var options = new JsonSerializerOptions
            {
                WriteIndented = true // ทำให้ JSON อ่านง่ายขึ้นโดยการจัดฟอร์แมตให้มีการเว้นบรรทัดและ indent
            };
            var jsonString = JsonSerializer.Serialize(cows, options);

            // บันทึก JSON string ลงในไฟล์
            System.IO.File.WriteAllText(JsonFilePath, jsonString);
        }
        [HttpPost]
        public JsonResult ResetCow([FromBody] Request request)
        {
            if (string.IsNullOrEmpty(request.CowId))
            {
                return Json(new { success = false, message = "รหัสวัวไม่ถูกต้อง" });
            }

            var cows = LoadCowsFromJson();
            var cow = cows.FirstOrDefault(c => c.id == request.CowId);

            if (cow == null)
            {
                return Json(new { success = false, message = "ไม่พบข้อมูลวัว" });
            }

            if (cow.IsBSOD)
            {
                cow.IsBSOD = false; // รีเซ็ตสถานะ BSOD
                cow.Color = "ขาว"; // หรือ "Brown" ตามสีเดิมของวัว
                cow.BottleCount = 0; // รีเซ็ตจำนวนขวดนม
                SaveCowsToJson(cows);

                return Json(new { success = true, message = "รีเซ็ตสถานะวัวสำเร็จ" });
            }
            else
            {
                return Json(new { success = false, message = "วัวไม่ได้อยู่ในสถานะ BSOD" });
            }
        }




    }

   
}
