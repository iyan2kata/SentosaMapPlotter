using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace myWpfWinApp01.Models
{
    public class Detail
    {
        public int id { get; set; }
        public string detailNodeId { get; set; }
        public string nodeId { get; set; }
        public string title { get; set; }
        public string title_zh_SG { get; set; }
        public string category { get; set; }
        public string videoUrl { get; set; }
        public string imageNameAndroid { get; set; }
        public string imageNameIOSx1 { get; set; }
        public string imageNameIOSx2 { get; set; }
        public string descriptionText { get; set; }
        public string descriptionText_zh_SG { get; set; }
        public string admission { get; set; }
        public string admission_zh_SG { get; set; }
        public string otherDetails { get; set; }
        public string otherDetails_zh_SG { get; set; }
        public string section { get; set; }
        public string section_zh_SG { get; set; }
        public string openingTimes { get; set; }
        public string openingTimes_zh_SG { get; set; }
        public string contactNumber { get; set; }
        public string email { get; set; }
        public string website { get; set; }
        public string type { get; set; }
        public int bookmarks { get; set; }
        public string linkedTicket { get; set; }
        public TimestampInformation updatedAt { get; set; }
    }
}
