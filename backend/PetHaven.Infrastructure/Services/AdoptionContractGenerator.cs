using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PetHaven.Application.Interfaces;
using PetHaven.Domain.Entities;

namespace PetHaven.Infrastructure.Services;

public class AdoptionContractGenerator : IAdoptionContractGenerator
{
    public Task<string> GenerateAsync(AdoptionApplication application, Animal animal, CancellationToken cancellationToken)
    {
        var root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "generated", "contracts");
        Directory.CreateDirectory(root);

        var fileName = $"adoption-{application.Id}.pdf";
        var fullPath = Path.Combine(root, fileName);

        using var document = new PdfDocument();
        document.Info.Title = "PetHaven Adoption Contract";

        var page = document.AddPage();
        var graphics = XGraphics.FromPdfPage(page);
        var titleFont = new XFont("DejaVu Sans", 18, XFontStyle.Bold);
        var labelFont = new XFont("DejaVu Sans", 11, XFontStyle.Bold);
        var valueFont = new XFont("DejaVu Sans", 11, XFontStyle.Regular);

        var y = 50d;
        graphics.DrawString("PetHaven Adoption Contract", titleFont, XBrushes.DarkSlateGray, new XRect(40, y, page.Width - 80, 30), XStringFormats.TopCenter);
        y += 55;

        DrawPair(graphics, labelFont, valueFont, "Application ID", application.Id.ToString(), y); y += 24;
        DrawPair(graphics, labelFont, valueFont, "Animal", $"{animal.Name} ({animal.Species}, {animal.Breed})", y); y += 24;
        DrawPair(graphics, labelFont, valueFont, "Animal ID", animal.Id.ToString(), y); y += 24;
        DrawPair(graphics, labelFont, valueFont, "Adopter Profile ID", application.AdopterProfileId.ToString(), y); y += 24;
        DrawPair(graphics, labelFont, valueFont, "Status", application.Status.ToString(), y); y += 24;
        DrawPair(graphics, labelFont, valueFont, "Generated At", DateTime.UtcNow.ToString("u"), y); y += 40;

        graphics.DrawString("Terms", labelFont, XBrushes.Black, 40, y); y += 20;
        graphics.DrawString("The adopter agrees to provide humane care, veterinary attention, safe housing, and post-adoption reports when requested by the shelter.", valueFont, XBrushes.Black, new XRect(40, y, page.Width - 80, 80), XStringFormats.TopLeft);
        y += 100;

        graphics.DrawLine(XPens.Black, 40, y, 250, y);
        graphics.DrawLine(XPens.Black, page.Width - 250, y, page.Width - 40, y);
        y += 16;
        graphics.DrawString("Shelter representative", valueFont, XBrushes.Black, 40, y);
        graphics.DrawString("Adopter", valueFont, XBrushes.Black, page.Width - 250, y);

        document.Save(fullPath);
        return Task.FromResult($"/generated/contracts/{fileName}");
    }

    private static void DrawPair(XGraphics graphics, XFont labelFont, XFont valueFont, string label, string value, double y)
    {
        graphics.DrawString($"{label}:", labelFont, XBrushes.Black, 40, y);
        graphics.DrawString(value, valueFont, XBrushes.Black, 180, y);
    }
}
