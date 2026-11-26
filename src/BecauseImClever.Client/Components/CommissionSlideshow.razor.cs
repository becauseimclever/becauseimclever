namespace BecauseImClever.Client.Components;

using BecauseImClever.Application.Commissions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

/// <summary>Manually controlled, keyboard- and touch-accessible project images.</summary>
public partial class CommissionSlideshow
{
    private IReadOnlyList<CommissionImageResponse>? previousImages;
    private double? touchX;
    private double touchY;

    [Parameter]
    public Guid ProductId { get; set; }

    [Parameter]
    public IReadOnlyList<CommissionImageResponse> Images { get; set; } = [];

    private int ActiveIndex { get; set; }

    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(this.previousImages, this.Images))
        {
            this.ActiveIndex = 0;
            this.previousImages = this.Images;
        }
    }

    private void Next() => this.ActiveIndex = (this.ActiveIndex + 1) % this.Images.Count;

    private void Previous() => this.ActiveIndex = (this.ActiveIndex + this.Images.Count - 1) % this.Images.Count;

    private void HandleKey(KeyboardEventArgs args)
    {
        if (args.Key == "ArrowRight")
        {
            this.Next();
        }
        else if (args.Key == "ArrowLeft")
        {
            this.Previous();
        }
    }

    private void StartTouch(TouchEventArgs args)
    {
        this.touchX = args.ChangedTouches.FirstOrDefault()?.ClientX;
        this.touchY = args.ChangedTouches.FirstOrDefault()?.ClientY ?? 0;
    }

    private void EndTouch(TouchEventArgs args)
    {
        var end = args.ChangedTouches.FirstOrDefault();
        if (this.touchX is double start && end is not null &&
            Math.Abs(end.ClientX - start) > 50 && Math.Abs(end.ClientX - start) > Math.Abs(end.ClientY - this.touchY))
        {
            if (end.ClientX < start)
            {
                this.Next();
            }
            else
            {
                this.Previous();
            }
        }

        this.touchX = null;
    }
}
