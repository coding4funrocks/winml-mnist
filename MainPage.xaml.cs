using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.AI.MachineLearning;
using Windows.Foundation;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MNIST_Demo
{
    public sealed partial class MainPage : Page
    {
        private Helper helper = new Helper();
        RenderTargetBitmap renderBitmap = new RenderTargetBitmap();

        private mnistInput ModelInput = new mnistInput();
        private mnistOutput ModelOutput;
        private mnistModel ModelGen;

        public MainPage()
        {
            InitializeComponent();
            
            // Set supported inking device types.
            InkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse | Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch;
            InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(
                new Windows.UI.Input.Inking.InkDrawingAttributes()
                {
                    Color = Windows.UI.Colors.White,
                    Size = new Size(22, 22),
                    IgnorePressure = true,
                    IgnoreTilt = true,
                }
            );
            LoadModelAsync();
        }

        private async Task LoadModelAsync()
        {
            // Load the machine learning model
            StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/mnist.onnx"));
            ModelGen = await mnistModel.CreateFromStreamAsync(modelFile as IRandomAccessStreamReference);
        }

        private async void RecognizeButtonClick(object sender, RoutedEventArgs e)
        {
            // Bind the model input with the contents from InkCanvas
            VideoFrame vf = await helper.GetHandWrittenImage(InkGrid);
            ModelInput.Input3 = ImageFeatureValue.CreateFromVideoFrame(vf);

            // Evaluate the model
            ModelOutput = await ModelGen.EvaluateAsync(ModelInput);

            // Convert the output to datatype
            IReadOnlyList<float> vectorImage = ModelOutput.Plus214_Output_0.GetAsVectorView();
            IList<float> imageList = vectorImage.ToList();

            // Query to check for highest probability digit
            var maxIndex = imageList.IndexOf(imageList.Max());

            // Display the results
            NumberLabel.Text = maxIndex.ToString();
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            InkCanvas.InkPresenter.StrokeContainer.Clear();
            NumberLabel.Text = "";
        }
    }
}
