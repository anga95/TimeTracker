using System.Globalization;
using TimeTracker.ViewModels;

namespace TimeTracker.Tests.ViewModels
{
    [TestFixture]
    public class MonthNavigationViewModelTests
    {
        private MonthNavigationViewModel _viewModel;
        private bool _stateChangedFired;
        private (int Year, int Month)? _navigationRequestedParams;

        [SetUp]
        public void Setup()
        {
            _viewModel = new MonthNavigationViewModel();
            _stateChangedFired = false;
            _navigationRequestedParams = null;
            
            _viewModel.StateChanged += () => _stateChangedFired = true;
            _viewModel.NavigationRequested += (params_) => 
            {
                _navigationRequestedParams = params_;
                return Task.CompletedTask;
            };
        }

        [Test]
        public void PropertySetter_Month_NotifiesStateChanged()
        {
            // Act
            _viewModel.Month = 5;
            
            // Assert
            Assert.That(_viewModel.Month, Is.EqualTo(5));
            Assert.That(_stateChangedFired, Is.True);
        }
        
        [Test]
        public void PropertySetter_Year_NotifiesStateChanged()
        {
            // Act
            _viewModel.Year = 2024;
            
            // Assert
            Assert.That(_viewModel.Year, Is.EqualTo(2024));
            Assert.That(_stateChangedFired, Is.True);
        }
        
        [Test]
        public void MonthName_ReturnsCorrectName()
        {
            // Arrange
            _viewModel.Month = 1;
            
            // Act & Assert
            Assert.That(_viewModel.MonthName, Is.EqualTo(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(1)));
        }
        
        [Test]
        public async Task PrevMonthClicked_WithJanuary_GoesToDecemberPreviousYear()
        {
            // Arrange
            _viewModel.Month = 1;
            _viewModel.Year = 2024;
            
            // Act
            await _viewModel.PrevMonthClicked();
            
            // Assert
            Assert.That(_navigationRequestedParams, Is.Not.Null);
            Assert.That(_navigationRequestedParams?.Year, Is.EqualTo(2023));
            Assert.That(_navigationRequestedParams?.Month, Is.EqualTo(12));
        }
        
        [Test]
        public async Task PrevMonthClicked_WithNonJanuary_GoesToPreviousMonth()
        {
            // Arrange
            _viewModel.Month = 5;
            _viewModel.Year = 2024;
            
            // Act
            await _viewModel.PrevMonthClicked();
            
            // Assert
            Assert.That(_navigationRequestedParams, Is.Not.Null);
            Assert.That(_navigationRequestedParams?.Year, Is.EqualTo(2024));
            Assert.That(_navigationRequestedParams?.Month, Is.EqualTo(4));
        }
        
        [Test]
        public async Task NextMonthClicked_WithDecember_GoesToJanuaryNextYear()
        {
            // Arrange
            _viewModel.Month = 12;
            _viewModel.Year = 2024;
            
            // Act
            await _viewModel.NextMonthClicked();
            
            // Assert
            Assert.That(_navigationRequestedParams, Is.Not.Null);
            Assert.That(_navigationRequestedParams?.Year, Is.EqualTo(2025));
            Assert.That(_navigationRequestedParams?.Month, Is.EqualTo(1));
        }
        
        [Test]
        public async Task NextMonthClicked_WithNonDecember_GoesToNextMonth()
        {
            // Arrange
            _viewModel.Month = 5;
            _viewModel.Year = 2024;
            
            // Act
            await _viewModel.NextMonthClicked();
            
            // Assert
            Assert.That(_navigationRequestedParams, Is.Not.Null);
            Assert.That(_navigationRequestedParams?.Year, Is.EqualTo(2024));
            Assert.That(_navigationRequestedParams?.Month, Is.EqualTo(6));
        }
    }
}