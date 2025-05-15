# MoonPress

[![Build status badge](https://github.com/blueheron786/moonpress/actions/workflows/dotnet.yml/badge.svg)](https://github.com/blueheron786/moonpress/actions/workflows/dotnet.yml) [![CodeCov code coverage badge](https://codecov.io/gh/blueheron786/moonpress/graph/badge.svg?token=66NBA3ZW9U)](https://codecov.io/gh/blueheron786/moonpress)

MoonPress is a static website generator, inspired by the amazing WordPress.

- **No Code:** Manage and create your site and content, entirely through a UI, without writing any code
- **Blazing Fast:** Deploy a blazing-fast static website - pure HTML and JS
- **Secure:** no user accounts to hack or databases to compromise

That's MoonPress.

At present, it only works for Windows, because it depends on Blazor Desktop and native WPF code for file/folder controls.

For advanced users:

- **Completely Customizable:** everything from themes to styles is within your control
- **Git Friendly:** Source files remain as JSON and Markdown, so you can merge changes easily

Built in C# and Blazor Desktop.

## How Does It Work?

A MoonPress project is a combination of input files: JSON configuration, Markdown posts, HTML themes, CSS, etc. When you generate the site, it generates the final static site files as plain ol' HTML.


## Why Blazor Desktop?

Take a look at the branches to see other technologies that didn't work, or didn't suit my needs.

- **Avalonia:** Extremely brittle, fails to build for many reasons, learning curve for web development.
- **Blazor Hybrid MAUI:** Includes code for building Android, iOS, etc. Requires lots of SDKs, and is still in preview
- **Electron:** Primarily uses non-.NET technologies
- **PWA:** Requires JS to get file paths, can't get absolute file paths

We can always revisit these options later, if there's a compelling reason to do something else.