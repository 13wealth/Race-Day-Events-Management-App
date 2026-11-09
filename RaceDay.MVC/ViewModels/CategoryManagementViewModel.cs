/***************************************************************************************
 *                         CATEGORY MANAGEMENT VIEW MODEL                              *
 * Holds the information required to manage categories belonging to an event.          *
 * It includes:                                                                        *
 *  1. Event information – identifies the event being managed.                         *
 *  2. Category information – holds the category name and ID.                          *
 *  3. Category list – stores the categories retrieved from the API.                   *
 ***************************************************************************************/

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Mvc.ViewModels
{
    public class CategoryManagementViewModel
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = "";

        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Please enter a category name.")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = "";

        public List<CategoryApiResponse> Categories { get; set; } = new List<CategoryApiResponse>();
    }

    /*
     * CATEGORY API RESPONSE                                  
     * Represents a category returned by the RaceDay API.                                  
     * JSON property attributes map the API's snake_case fields to C# properties.          
     */
    public class CategoryApiResponse
    {
        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("event_id")]
        public int EventId { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; } = "";
    }
}