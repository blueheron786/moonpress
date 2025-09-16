namespace MoonPress.Core.Templates;

/// <summary>
/// Template renderer for category pages that loads templates from theme directories
/// </summary>
public class CategoryPageTemplate
{
    private const string DefaultTemplate = @"<h1>Category: {{categoryName}}</h1>
<ul>
{{#items}}
  <li><a href=""{{url}}"">{{title}}</a>{{#summary}} - <em>{{summary}}</em>{{/summary}}</li>
{{/items}}
</ul>";

    private const string CategoryTemplateFileName = "category-page.html";

    public string Render(string categoryName, IEnumerable<CategoryPageItem> items, string? themePath = null)
    {
        var template = LoadTemplate(themePath);
        
        // Replace category name
        var html = template.Replace("{{categoryName}}", categoryName);
        
        // Handle items loop
        var itemsContent = string.Empty;
        var itemTemplate = ExtractItemTemplate(template);
        
        foreach (var item in items)
        {
            var itemHtml = itemTemplate
                .Replace("{{url}}", item.Url)
                .Replace("{{title}}", item.Title);
                
            // Handle date replacement
            if (item.DatePublished.HasValue)
            {
                itemHtml = itemHtml.Replace("{{date}}", item.DatePublished.Value.ToString("MMMM dd, yyyy"));
            }
            else
            {
                itemHtml = itemHtml.Replace("{{date}}", "");
            }
                
            // Handle conditional summary
            if (!string.IsNullOrWhiteSpace(item.Summary))
            {
                itemHtml = itemHtml.Replace("{{#summary}}", "").Replace("{{/summary}}", "");
                itemHtml = itemHtml.Replace("{{summary}}", item.Summary);
            }
            else
            {
                itemHtml = RemoveConditionalSection(itemHtml, "{{#summary}}", "{{/summary}}");
            }
            
            itemsContent += itemHtml + Environment.NewLine;
        }
        
        return ReplaceItemsSection(html, itemsContent);
    }

    private string LoadTemplate(string? themePath)
    {
        if (string.IsNullOrEmpty(themePath))
        {
            return DefaultTemplate;
        }

        var templatePath = Path.Combine(themePath, "templates", CategoryTemplateFileName);
        
        if (File.Exists(templatePath))
        {
            try
            {
                return File.ReadAllText(templatePath);
            }
            catch (Exception ex)
            {
                // Log error and fall back to default template
                Console.WriteLine($"Warning: Could not load category template from '{templatePath}': {ex.Message}");
                Console.WriteLine("Falling back to default template.");
                return DefaultTemplate;
            }
        }
        
        return DefaultTemplate;
    }
    
    private static string ExtractItemTemplate(string template)
    {
        var start = template.IndexOf("{{#items}}") + "{{#items}}".Length;
        var end = template.IndexOf("{{/items}}");
        return template.Substring(start, end - start).Trim();
    }
    
    private static string ReplaceItemsSection(string template, string itemsContent)
    {
        var start = template.IndexOf("{{#items}}");
        var end = template.IndexOf("{{/items}}") + "{{/items}}".Length;
        return template.Remove(start, end - start).Insert(start, itemsContent);
    }
    
    private static string RemoveConditionalSection(string text, string startTag, string endTag)
    {
        var start = text.IndexOf(startTag);
        if (start == -1) return text;
        
        var end = text.IndexOf(endTag, start) + endTag.Length;
        return text.Remove(start, end - start);
    }
}

public record CategoryPageItem(string Url, string Title, string? Summary = null, DateTime? DatePublished = null);