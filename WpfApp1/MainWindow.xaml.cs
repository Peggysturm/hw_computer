using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private string correctAnswer;
        private string explanation;
        private Dictionary<string, bool> testFlags;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTestFlags();
        }

        private void InitializeTestFlags()
        {
            testFlags = new Dictionary<string, bool>
            {
                { "CpuSocketTest", false },
                { "RamTest", false },
                { "PcieTest", false },
                { "BiosTest", false },
                { "QuartzTest", false },
                { "BiosBatteryTest", false },
                { "12VTest", false },
                { "5VTest", false },
                { "3_3VTest", false },
                { "UsbTest", false },
                { "ResistanceTest", false }
            };
        }

        private void PlaySound(string soundFilePath)
        {
            try
            {
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri($"pack://siteoforigin:,,,/{soundFilePath}"));
                player.Play();
            }
            catch (Exception ex)
            {
                OutputTextBlock.Text = $"Ошибка при воспроизведении звука: {ex.Message}";
            }
        }

        private void TestMethodToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton clickedButton = sender as ToggleButton;
            ToggleButtonsOffExcept(clickedButton);
            clickedButton.IsChecked = true;
            ResetFlagsAndUI();
        }

        private void ToggleButtonsOffExcept(ToggleButton clickedButton)
        {
            foreach (var button in new[] { Resistance_btn, Voltage_Click_btn, Current_btn, oscilloscope_btn, ram_tester_button, Pcie_tester_button, cpu_socket_tester_button })
            {
                if (button != clickedButton)
                    button.IsChecked = false;
            }
        }

        private void ResetFlagsAndUI()
        {
            foreach (var key in testFlags.Keys.ToList())
            {
                testFlags[key] = false;
            }
            HideQuestionAndAnswers();
            OscilloscopeImage.Visibility = Visibility.Collapsed;
        }

        private void Voltage3_3V_Click(object sender, RoutedEventArgs e)
        {
            PerformTest(Voltage_Click_btn, 3.3, 3.0, 3.6, "линии 3.3V", "+3,3 ± 0,3 вольт", Resistance_btn, 100, 10000, "от 100 Ом до 10 кОм");
        }

        private void Voltage5V_Click(object sender, RoutedEventArgs e)
        {
            PerformTest(Voltage_Click_btn, 5, 4.5, 5.5, "линии 5V", "+5 ± 0,5 вольт", Resistance_btn, 100, 10000, "от 100 Ом до 10 кОм");
        }

        private void Voltage12V_Click(object sender, RoutedEventArgs e)
        {
            PerformTest(Voltage_Click_btn, 12, 11.8, 13.2, "линии 12V", "+12V ± 1,2 вольта", Resistance_btn, 1000, 20000, "от 1 кОм до 20 кОм");
        }

        private void PerformTest(ToggleButton primaryButton, double nominalVoltage, double minVoltage, double maxVoltage, string voltageLine, string voltageRange, ToggleButton secondaryButton, double minResistance, double maxResistance, string resistanceRange)
        {
            if (primaryButton.IsChecked == true)
            {
                TestVoltage(nominalVoltage, minVoltage, maxVoltage, voltageLine, voltageRange);
            }
            else if (secondaryButton.IsChecked == true)
            {
                TestResistance(minResistance, maxResistance, voltageLine, resistanceRange);
            }
            else
            {
                OutputTextBlock.Text = "НЕПРАВИЛЬНО!!! ПОПРОБУЙ ЕЩЁ РАЗ!!!";
                PlaySound("Resources/wrong.mp3");
            }
        }

        private void TestVoltage(double nominalVoltage, double minVoltage, double maxVoltage, string lineName, string normalRange)
        {
            double voltage = GenerateRandomVal(nominalVoltage - 1, nominalVoltage + 1, minVoltage, maxVoltage);
            OutputTextBlock.Text = $"Напряжение {lineName}: {voltage:F1} В";
            EvaluateTestResult(voltage >= minVoltage && voltage <= maxVoltage, lineName, normalRange);
        }

        private void TestResistance(double minResistance, double maxResistance, string lineName, string normalRange)
        {
            double resistance = GenerateRandomVal(0, minResistance, minResistance, maxResistance);
            OutputTextBlock.Text = $"Сопротивление {lineName}: {resistance:F1} Ом";
            EvaluateTestResult(resistance >= minResistance && resistance <= maxResistance, lineName, normalRange);
        }

        private double GenerateRandomVal(double lowRangeStart, double lowRangeEnd, double highRangeStart, double highRangeEnd)
        {
            Random rand = new Random();
            return (rand.Next(2) == 0) ? lowRangeStart + rand.NextDouble() * (lowRangeEnd - lowRangeStart) : highRangeStart + rand.NextDouble() * (highRangeEnd - highRangeStart);
        }

        private void EvaluateTestResult(bool isWithinRange, string lineName, string normalRange)
        {
            correctAnswer = isWithinRange ? "Да" : "Нет";
            explanation = $"Так как нормальные значения для {lineName} должны быть в диапазоне {normalRange}.";
            ShowQuestionAndAnswers();
        }

        private void SocketButton_Click(object sender, RoutedEventArgs e)
        {
            PerformComponentTest(cpu_socket_tester_button, "сокета ЦПУ", new[] { "Все индикаторы горят красным", "Часть индикаторов не горит", "Ни один из индикаторов не горит" }, "при исправном сокете процессора все индикаторы должны гореть красным.");
        }

        private void RamButton_Click(object sender, RoutedEventArgs e)
        {
            PerformComponentTest(ram_tester_button, "слота ОЗУ", new[] { "Все индикаторы горят красным", "Часть индикаторов не горит", "Ни один из индикаторов не горит" }, "при исправном слоте ОЗУ все индикаторы должны гореть красным.");
        }

        private void PcieButton_Click(object sender, RoutedEventArgs e)
        {
            PerformComponentTest(Pcie_tester_button, "слота PCIe", new[] { "Есть сигнал", "Нет сигнала" }, "должен быть сигнал.");
        }

        private void PerformComponentTest(ToggleButton testerButton, string element, string[] messages, string explanation)
        {
            if (testerButton.IsChecked == true)
            {
                TestRandom(element, messages, "Да", "Нет", explanation);
            }
            else
            {
                OutputTextBlock.Text = "НЕПРАВИЛЬНО!!! ПОПРОБУЙ ЕЩЁ РАЗ!!!";
                PlaySound("Resources/wrong.mp3");
            }
        }

        private void TestRandom(string element, string[] messages, string correctMsg, string incorrectMsg, string additionalExplanation)
        {
            Random rand = new Random();
            int index = rand.Next(messages.Length);
            OutputTextBlock.Text = messages[index];
            correctAnswer = (index == 0) ? correctMsg : incorrectMsg;
            explanation = $"Так как {additionalExplanation}";
            ShowQuestionAndAnswers();
        }

        private void BiosButton_Click(object sender, RoutedEventArgs e)
        {
            PerformOscilloscopeTest(oscilloscope_btn, "BIOS");
        }

        private void QuartzButton_Click(object sender, RoutedEventArgs e)
        {
            PerformOscilloscopeTest(oscilloscope_btn, "кварцевого резонатора");
        }

        private void PerformOscilloscopeTest(ToggleButton testerButton, string element)
        {
            if (testerButton.IsChecked == true)
            {
                TestOscilloscope(element, "так как нет осциллограммы.", "так как есть осциллограмма.");
            }
            else
            {
                OutputTextBlock.Text = "НЕПРАВИЛЬНО!!! ПОПРОБУЙ ЕЩЁ РАЗ!!!";
            }
        }

        private void TestOscilloscope(string element, string incorrectExplanation, string correctExplanation)
        {
            string[] images = { "pack://application:,,,/faultyoscilloscope.jpg", "pack://application:,,,/workingoscilloscope.jpg" };
            Random rand = new Random();
            int index = rand.Next(images.Length);

            OscilloscopeImage.Source = new BitmapImage(new Uri(images[index]));
            OscilloscopeImage.Visibility = Visibility.Visible;

            correctAnswer = (index == 0) ? "Нет" : "Да";
            explanation = (index == 0) ? incorrectExplanation : correctExplanation;

            ShowQuestionAndAnswers();
        }

        private void BiosBatteryButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSingleTest(Voltage_Click_btn, 3.0, 2.7, 3.3, "батарейки BIOS", "+3 ± 0,3 вольт");
        }

        private void UsbButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSingleTest(Voltage_Click_btn, 5.0, 4.75, 5.25, "линии USB", "5V ± 0,25V");
        }

        private void PerformSingleTest(ToggleButton testerButton, double nominalVoltage, double minVoltage, double maxVoltage, string lineName, string normalRange)
        {
            if (testerButton.IsChecked == true)
            {
                TestVoltage(nominalVoltage, minVoltage, maxVoltage, lineName, normalRange);
            }
            else
            {
                OutputTextBlock.Text = "НЕПРАВИЛЬНО!!! ПОПРОБУЙ ЕЩЁ РАЗ!!!";
                PlaySound("Resources/wrong.mp3");
            }
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            string userAnswer = (sender as Button).Content.ToString();
            string resultMessage = (userAnswer == correctAnswer) ? "Правильный ответ!" : "Неправильный ответ.";
            resultMessage += $" {explanation}";
            OutputTextBlock.Text = resultMessage;
            HideQuestionAndAnswers();
        }

        private void ShowQuestionAndAnswers()
        {
            QuestionTextBlock.Text = "Правильно ли работает этот элемент?";
            YesButton.Visibility = Visibility.Visible;
            NoButton.Visibility = Visibility.Visible;
        }

        private void HideQuestionAndAnswers()
        {
            QuestionTextBlock.Text = string.Empty;
            YesButton.Visibility = Visibility.Collapsed;
            NoButton.Visibility = Visibility.Collapsed;
            OscilloscopeImage.Visibility = Visibility.Collapsed;
        }
    }
}
