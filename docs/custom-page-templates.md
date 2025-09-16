# Creating Custom Page Templates in MoonPress

This guide shows you how to create custom page layouts for specific types of content using MoonPress's built-in templating system.

## What Are Custom Page Templates?

Custom page templates allow you to create special layouts for specific pages or content types using MoonPress's `{{posts}}` templating system. For example:

- A **books page** that displays your book reviews in a grid layout
- A **projects page** that showcases your work with special styling  
- A **home page** that shows recent content from multiple categories

All of this is done with template files and content - no code required!

## Quick Start

### 1. Create Your Content Structure

First, organize your content into logical categories. For a book review site:

```
content/
  books/
    amazing-book.md
    great-novel.md
    productivity-guide.md
  pages/
    books.md        # Your custom books listing page
    index.md        # Your home page
themes/
  your-theme/
    books-list.html # Custom template for books
```

### 2. Create Book Content

Create individual book reviews in `content/books/`:

```markdown
---
title: The Amazing Book
slug: amazing-book
category: books
datePublished: 2025-09-15 10:00:00
author: Jane Author
rating: 5
isbn: 978-0123456789
summary: This book completely changed how I think about productivity and life balance.
---

# The Amazing Book

## My Review

This book was incredible because it tackles the fundamental question of how we spend our time...

## Key Takeaways

- Time is our most valuable resource
- Small habits compound over time
- Focus on systems, not goals

## Would I Recommend It?

Absolutely! This is a must-read for anyone looking to improve their productivity.
```

### 3. Create a Custom Template

Create a template file at `themes/your-theme/books-list.html`:

```html
<div class="page-intro">
  {{content}}
</div>

<div class="books-grid">
  {{posts | category=books}}
    <div class="book-card">
      <h3><a href="{{url}}">{{title}}</a></h3>
      <div class="book-meta">
        {{#author}}by {{author}}{{/author}}
        {{#rating}} • ⭐ {{rating}}/5{{/rating}}
        {{#date}} • Read {{date}}{{/date}}
      </div>
      {{#summary}}<p class="book-summary">{{summary}}</p>{{/summary}}
    </div>
  {{/posts}}
</div>

<style>
.page-intro {
  margin-bottom: 2rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 8px;
}

.books-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 2rem;
  margin-top: 2rem;
}

.book-card {
  border: 1px solid #e0e0e0;
  border-radius: 12px;
  padding: 1.5rem;
  background: white;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
  transition: all 0.2s ease;
}

.book-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 16px rgba(0,0,0,0.1);
}

.book-card h3 {
  margin: 0 0 0.75rem 0;
  font-size: 1.25rem;
}

.book-card h3 a {
  text-decoration: none;
  color: #2c3e50;
}

.book-card h3 a:hover {
  color: #3498db;
}

.book-meta {
  color: #666;
  font-size: 0.9rem;
  margin-bottom: 1rem;
  font-weight: 500;
}

.book-summary {
  color: #555;
  line-height: 1.6;
  margin: 0;
}
</style>
```

### 4. Create Your Books Page

Create a books listing page at `content/pages/books.md`:

```markdown
---
title: My Book Reviews
slug: books
template: books-list
---

# My Book Reviews

Welcome to my collection of book reviews! I read across various genres and share my honest thoughts on each book.

## What You'll Find Here

- Detailed reviews with ratings
- Key takeaways from each book
- Recommendations for different audiences
- Links to purchase the books
```

## Template Features

### Available Template Variables

In your custom templates, you can use these variables:

**Standard Variables:**
- `{{content}}` - The page's own markdown content
- `{{title}}` - Page title
- `{{url}}` - Link to the content item
- `{{category}}` - Content category  
- `{{summary}}` - Content summary
- `{{date}}` - Publication date

**Custom Field Variables:**
Any field you add to your frontmatter becomes a template variable:
- `{{author}}` - Author name
- `{{rating}}` - Book rating
- `{{isbn}}` - ISBN number
- `{{genre}}` - Book genre
- etc.

### Conditional Sections

Use conditional sections to only show content when a field has a value:

```html
{{#author}}by {{author}}{{/author}}
{{#rating}} • ⭐ {{rating}}/5{{/rating}}
{{#summary}}<p>{{summary}}</p>{{/summary}}
```

If the field is empty, the entire section (including the surrounding text) won't appear.

### Posts Blocks

Use `{{posts}}` blocks to display collections of content:

```html
{{posts | category=books}}
  <div class="book-item">
    <h3>{{title}}</h3>
    <p>{{summary}}</p>
  </div>
{{/posts}}
```

**Available Filters:**
- `category=books` - Only show books
- `limit=5` - Only show first 5 items
- Combine filters: `{{posts | category=books | limit=3}}`

## More Examples

### Simple Projects Template

Create `themes/your-theme/projects-list.html`:

```html
{{content}}

<div class="projects-grid">
  {{posts | category=projects}}
    <div class="project-card">
      <h3><a href="{{url}}">{{title}}</a></h3>
      {{#tech}}<div class="tech-stack">Built with: {{tech}}</div>{{/tech}}
      {{#demo}}<a href="{{demo}}" class="demo-link">View Demo</a>{{/demo}}
      {{#github}}<a href="{{github}}" class="github-link">View Code</a>{{/github}}
      {{#summary}}<p>{{summary}}</p>{{/summary}}
    </div>
  {{/posts}}
</div>

<style>
.projects-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 2rem;
}

.project-card {
  border: 1px solid #ddd;
  padding: 1.5rem;
  border-radius: 8px;
  background: white;
}

.tech-stack {
  color: #666;
  font-size: 0.9rem;
  margin: 0.5rem 0;
}

.demo-link, .github-link {
  display: inline-block;
  padding: 0.5rem 1rem;
  margin: 0.25rem;
  background: #007acc;
  color: white;
  text-decoration: none;
  border-radius: 4px;
}
</style>
```

Then use it in your projects page:

```markdown
---
title: My Projects
slug: projects
template: projects-list
---

# My Projects

Here are some of the projects I've worked on.
```

### Recipe Template

Create `themes/your-theme/recipes-list.html`:

```html
{{content}}

<div class="recipes-grid">
  {{posts | category=recipes}}
    <div class="recipe-card">
      <h3><a href="{{url}}">{{title}}</a></h3>
      {{#cookTime}}<div class="cook-time">⏱️ {{cookTime}}</div>{{/cookTime}}
      {{#difficulty}}<div class="difficulty">📊 {{difficulty}}</div>{{/difficulty}}
      {{#cuisine}}<div class="cuisine">🌍 {{cuisine}}</div>{{/cuisine}}
      {{#summary}}<p>{{summary}}</p>{{/summary}}
    </div>
  {{/posts}}
</div>
```

## Complete Example

### File Structure
```
your-site/
  content/
    books/
      amazing-book.md
      great-novel.md
    pages/
      books.md
      index.md
  themes/
    your-theme/
      books-list.html
      home-with-books.html
      layout.html
```

### Book Content (`content/books/amazing-book.md`)
```markdown
---
title: The Amazing Book
slug: amazing-book
category: books
datePublished: 2025-09-15 10:00:00
author: Jane Author
rating: 5
isbn: 978-0123456789
genre: Self-Help
pages: 240
summary: This book completely changed how I think about productivity.
---

# The Amazing Book

## My Review
This book was incredible...
```

### Books Page (`content/pages/books.md`)
```markdown
---
title: My Book Reviews
slug: books
template: books-list
---

# My Book Reviews
Welcome to my book review collection!
```

### Home Page (`content/pages/index.md`)
```markdown
---
title: Welcome
slug: index
template: home-with-books
---

# Welcome to My Site
This is my personal blog where I share book reviews and thoughts.
```

### Custom Template (`themes/your-theme/books-list.html`)
```html
<div class="page-intro">
  {{content}}
</div>

<div class="books-grid">
  {{posts | category=books}}
    <div class="book-card">
      <h3><a href="{{url}}">{{title}}</a></h3>
      <div class="book-meta">
        {{#author}}by {{author}}{{/author}}
        {{#rating}} • ⭐ {{rating}}/5{{/rating}}
        {{#genre}} • {{genre}}{{/genre}}
      </div>
      {{#summary}}<p>{{summary}}</p>{{/summary}}
    </div>
  {{/posts}}
</div>

<style>
/* Your custom styling here */
</style>
```

That's it! No code required - just templates and content files.

## Key Takeaways

- Time is our most valuable resource
- Small habits compound over time
- Focus on systems, not goals

## Would I Recommend It?

Absolutely! This is a must-read for anyone looking to improve their productivity.
```

### 3. Create Your Custom Books Page

Create a special books listing page at `content/pages/books.md`:

```markdown
---
title: My Book Reviews
slug: books
template: books-list
---

# My Book Reviews

Welcome to my collection of book reviews! I read across various genres and share my honest thoughts on each book.

## What You'll Find Here

- Detailed reviews with ratings
- Key takeaways from each book
- Recommendations for different audiences
- Links to purchase the books
```

### 4. Enable the Template

The key is the `template: books-list` line in your frontmatter. This tells MoonPress to use a special template for this page that will:

- Display your intro content (the markdown you wrote)
- Automatically find all content with `category: books`
- Display them in a beautiful grid layout
- Show ratings, authors, and summaries
- Link to individual book review pages

## Content Structure Examples

### Book Review Structure

```markdown
---
title: Book Title
slug: book-title  
category: books
datePublished: 2025-09-15 10:00:00
author: Author Name
rating: 4
genre: Science Fiction
pages: 320
isbn: 978-0123456789
summary: Brief description that appears in listings
---

# Your Review Content Here
```

### Books Listing Page

```markdown
---
title: All My Books
slug: books
template: books-list
---

# Introduction content here

This content appears above the books grid.
```

### Enhanced Home Page

```markdown
---
title: Welcome to My Site
slug: index  
template: home-with-books
---

# Welcome!

Your regular home page content here.

The recent books section will appear below this content automatically.
```