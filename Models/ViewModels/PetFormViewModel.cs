using Microsoft.AspNetCore.Mvc.Rendering;

namespace PetPhotographyApp.Models.ViewModels
{
    public class PetFormViewModel
    {
        public int? PetId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;

        public int OwnerId { get; set; }

        // 用于下拉列表选择主人
        public IEnumerable<SelectListItem> Owners { get; set; } = new List<SelectListItem>();
    }
}
