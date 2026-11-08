using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts
{
    public class CategoryRequest
    {
        [Required]
        public string CategoryName { get; set; } = "";
    }

    public class CategoryResponse
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = "";
    }

    public class CategoryListResponse
    {
        public List<CategoryResponse> Categories { get; set; } = [];
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }

    public class CategoryCreateResponse
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = "";
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }

    public class CategoryUpdateResponse
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = "";
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }

    public class CategoryDetailResponse
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = "";
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }

    public class CategoryDeleteResponse
    {
        public string Error { get; set; } = "";
        public int StatusCode { get; set; }
    }
}