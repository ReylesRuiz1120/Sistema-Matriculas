using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TestProject.Helpers
{
    // Implementación simple de ITempDataProvider para pruebas unitarias.
    public class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(Microsoft.AspNetCore.Http.HttpContext context)
        {
            return new Dictionary<string, object>();
        }

        public void SaveTempData(Microsoft.AspNetCore.Http.HttpContext context, IDictionary<string, object> values)
        {
            // no-op
        }
    }
}
