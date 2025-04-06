// Required Avalonia namespaces for UI elements, layout, and input
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

// Standard .NET namespaces
using System;
using System.Diagnostics;
using System.Reflection; // Needed to access versioning metadata

// Partial class definition for the AboutWindow, inherits from Avalonia's Window
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        // Set the title and size of the window
        this.Title = "About";
        this.Width = 300;
        this.Height = 200;
        this.Position = new PixelPoint(50, 300); // Window screen position (X, Y)

        // Create a vertically-stacked layout container with padding and centered alignment
        var mainStackPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(20)
        };

        // Retrieve the version string from the assembly metadata, fallback to "Unknown Version" if not found
        string version = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "Unknown Version";

        // Text block that displays the game title, version, and creator label
        var textBlock = new TextBlock
        {
            Text = $"Bubble Swerve\n\nVersion: {version}\nCreated by:",
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10) // Adds space below the text
        };

        // Clickable link styled like a hyperlink, pointing to the creator's website
        var linkTextBlock = new TextBlock
        {
            Text = "https://scott.rodeo/",
            TextAlignment = TextAlignment.Center,
            Foreground = Brushes.Blue,                        // Blue text
            TextDecorations = TextDecorations.Underline,      // Underlined
            Cursor = new Cursor(StandardCursorType.Hand)      // Hand cursor on hover
        };

        // Handle mouse click on the link to open the URL in the default browser
        linkTextBlock.PointerPressed += (sender, e) =>
        {
            OpenUrl("https://scott.rodeo/"); // Opens the link
        };

        // Add both the text and the link to the layout container
        mainStackPanel.Children.Add(textBlock);
        mainStackPanel.Children.Add(linkTextBlock);

        // Set the window content to be the fully populated stack panel
        this.Content = mainStackPanel;
    }

    // Method to open a URL in the user's default browser
    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,        // Target URL
                UseShellExecute = true // Required for opening URLs cross-platform
            });
        }
        catch
        {
            // Print to console if something goes wrong (e.g., no default browser)
            Console.WriteLine($"Failed to open URL: {url}");
        }
    }
}
