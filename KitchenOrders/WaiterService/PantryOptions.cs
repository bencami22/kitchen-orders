using System.ComponentModel.DataAnnotations;

namespace KitchenOrders
{
    public class PantryOptions
    {
        public const string SectionIdentifier = "PantryOptions";
        [Required]
        public string PantryId { get; set; }
    }
}