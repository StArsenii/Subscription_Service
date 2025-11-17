using Moq;
using Subscription_Service.Models;
using Subscription_Service.Services;
using Subscription_Service.Services.Interfaces;
using Xunit;

namespace SubscriptionServiceTests
{
    public class SubscriptionServiceTests
    {

        private readonly Mock<IMemberRepository> _repo = new();
        private readonly Mock<IPaymentService> _payment = new();
        private readonly Mock<INotificationService> _notify = new();
        private readonly SubscriptionService _service;

        public SubscriptionServiceTests()
        {
            _service = new SubscriptionService(_repo.Object, _payment.Object, _notify.Object);
        }

        /// <summary>
        /// Параметризований тест для перевірки успішного поновлення підписки на різну кількість днів.
        /// Типи перевірок: [Theory], [InlineData], Assert.True
        /// </summary>
        [Theory]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(365)]
        public void RenewSubscription_ShouldExtendSubscription_WhenPaymentSucceeds(int days)
        {

            var member = new Member { Id = 1, IsActive = false, SubscriptionEnd = DateTime.Today };
            _repo.Setup(r => r.GetById(1)).Returns(member);
            _payment.Setup(p => p.VerifyPayment(1, 100)).Returns(true);


            bool result = _service.RenewSubscription(1, 100, days);


            Assert.True(result);
            Assert.True(member.IsActive);

            Assert.Equal(DateTime.Today.AddDays(days).Date, member.SubscriptionEnd?.Date);
        }

        /// <summary>
        /// Тест перевіряє, що метод кидає виняток, якщо користувача не знайдено.
        /// Типи перевірок: Assert.Throws
        /// </summary>
        [Fact]
        public void RenewSubscription_ShouldThrowException_WhenMemberNotFound()
        {

            _repo.Setup(r => r.GetById(It.IsAny<int>())).Returns((Member)null);


            Assert.Throws<ArgumentException>(() => _service.RenewSubscription(99, 50, 30));
        }

        /// <summary>
        /// Тест перевіряє, що підписка не поновлюється при невдалій оплаті.
        /// Типи перевірок: Assert.False, Assert.NotEqual, Verify (Times.Never)
        /// </summary>
        [Fact]
        public void RenewSubscription_ShouldFail_WhenPaymentFails()
        {

            var originalDate = DateTime.Today.AddDays(-10);
            var member = new Member { Id = 1, IsActive = false, SubscriptionEnd = originalDate };

            _repo.Setup(r => r.GetById(1)).Returns(member);
            _payment.Setup(p => p.VerifyPayment(1, 100)).Returns(false);


            bool result = _service.RenewSubscription(1, 100, 30);


            Assert.False(result);
            Assert.Equal(originalDate, member.SubscriptionEnd);
            Assert.NotEqual(DateTime.Today.AddDays(30), member.SubscriptionEnd);

            _repo.Verify(r => r.Update(It.IsAny<Member>()), Times.Never);
        }

        /// <summary>
        /// Тест перевіряє, що деактивація спрацьовує тільки для протермінованих користувачів.
        /// Типи перевірок: Verify (Times.Exactly), It.Is (предикат)
        /// </summary>
        [Fact]
        public void DeactivateExpiredMembers_ShouldDeactivateOnlyExpired()
        {

            var expiredMember = new Member { Id = 1, IsActive = true, SubscriptionEnd = DateTime.Today.AddDays(-1) };
            var activeMember = new Member { Id = 2, IsActive = true, SubscriptionEnd = DateTime.Today.AddDays(5) };

            _repo.Setup(r => r.GetAll()).Returns(new List<Member> { expiredMember, activeMember });


            _service.DeactivateExpiredMembers();

            _repo.Verify(r => r.Update(It.IsAny<Member>()), Times.Exactly(1));


            _notify.Verify(n => n.SendNotification(It.IsAny<string>(), It.Is<int>(id => id == 1)), Times.Once);
        }

        /// <summary>
        /// Тест перевіряє, що нічого не відбувається, якщо список порожній.
        /// Типи перевірок: Assert.Empty, Verify (Times.Never)
        /// </summary>
        [Fact]
        public void DeactivateExpiredMembers_ShouldDoNothing_WhenListIsEmpty()
        {

            var emptyList = new List<Member>();
            _repo.Setup(r => r.GetAll()).Returns(emptyList);


            _service.DeactivateExpiredMembers();


            Assert.Empty(emptyList);
            _repo.Verify(r => r.Update(It.IsAny<Member>()), Times.Never);
        }
    }
}