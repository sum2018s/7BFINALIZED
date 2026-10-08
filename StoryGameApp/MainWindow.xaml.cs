using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StoryGameApp;

public partial class MainWindow : Window
{
    // Each Slide keeps its text, image and description together.
    private record Slide(string Text, string ImagePath, string Description);
    private readonly Slide[] slides =
    {
        new Slide("A signal reaches the next crew. What do they need to know?", "Assets/Images/01.png", "A signal card: the story begins."),
        new Slide("The crew compares the route with the last reliable observation.", "Assets/Images/02.png", "A route card: inspect the problem."),
        new Slide("Leave a clear account for the next reader. Your story continues here.", "Assets/Images/03.png", "A handoff card: record the result.")
    };
    private int currentSlide = 0; // Array positions start at zero.

    public MainWindow()
    {
        InitializeComponent(); // Build the named controls from XAML first.
        ShowSlide(currentSlide);
    }

    private void ShowSlide(int index)
    {
        if (index < 0 || index >= slides.Length) return;
        currentSlide = index;
        Slide slide = slides[currentSlide];
        StoryTextBlock.Text = slide.Text;
        ImageDescription.Text = slide.Description;
        SlideCounter.Text = $"Slide {currentSlide + 1} of {slides.Length}";
        BackButton.IsEnabled = currentSlide > 0;
        NextButton.IsEnabled = currentSlide < slides.Length - 1;
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, slide.ImagePath);
            if (!File.Exists(path)) throw new FileNotFoundException("Image file was not found.", path);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            StoryImage.Source = image;
            StatusText.Text = "Story ready.";
        }
        catch (Exception ex) when (ex is IOException || ex is NotSupportedException || ex is System.IO.FileFormatException)
        {
            StoryImage.Source = null;
            StatusText.Text = "Image unavailable. Check Assets/Images and the filename. " + ex.Message;
        }
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSlide(currentSlide + 1);
    }
    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSlide(currentSlide - 1);
    }
    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSlide(0);
    }
    // DAY 7B: paste CaptureHandlers.cs.txt HERE, inside these class braces.
    private BitmapSource? capturedImage;

    private void CaptureButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            capturedImage = CaptureService.CapturePrimaryScreen();
            CapturePreview.Source = capturedImage;
            SaveCaptureButton.IsEnabled = true;
            ClearCaptureButton.IsEnabled = true;
            StatusText.Text = "Capture ready. Inspect it before saving or sharing.";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Capture failed: " + ex.Message;
        }
    }

    private void SaveCaptureButton_Click(object sender, RoutedEventArgs e)
    {
        if (capturedImage is null) return;
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PNG image|*.png",
            DefaultExt = ".png",
            AddExtension = true,
            FileName = "story-capture.png",
            OverwritePrompt = true
        };
        if (dialog.ShowDialog(this) != true)
        {
            StatusText.Text = "Save cancelled. Preview kept.";
            return;
        }
        try
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(capturedImage));
            using var file = File.Create(dialog.FileName);
            encoder.Save(file);
            StatusText.Text = "PNG saved: " + dialog.FileName;
        }
        catch (Exception ex)
        {
            StatusText.Text = "Save failed. Choose a writable folder. " + ex.Message;
        }
    }

    private void ClearCaptureButton_Click(object sender, RoutedEventArgs e)
    {
        capturedImage = null;
        CapturePreview.Source = null;
        SaveCaptureButton.IsEnabled = false;
        ClearCaptureButton.IsEnabled = false;
        StatusText.Text = "Preview cleared. Story position kept.";
    }
}
