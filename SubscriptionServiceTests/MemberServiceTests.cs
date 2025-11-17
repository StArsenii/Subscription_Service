using Moq;
using Subscription_Service.Models;
using Subscription_Service.Services;
using Subscription_Service.Services.Interfaces;
using Xunit;

namespace SubscriptionServiceTests
{
    public class MemberServiceTests
    {

        private readonly Mock<IMemberRepository> _repo = new();
        private readonly MemberService _service;

        public MemberServiceTests()
        {
            _service = new MemberService(_repo.Object);
        }

        /// <summary>
        /// Тест перевіряє, що GetMember повертає об'єкт, якщо він існує.
        /// Типи перевірок: Assert.NotNull, Assert.Equal
        /// </summary>
        [Fact]
        public void GetMember_ShouldReturnMember_WhenExists()
        {

            var member = new Member { Id = 1, Name = "Test User" };
            _repo.Setup(r => r.GetById(1)).Returns(member);


            var result = _service.GetMember(1);


            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test User", result.Name);
        }

        /// <summary>
        /// Тест перевіряє, що GetMember повертає null для неіснуючого ID.
        /// Типи перевірок: Assert.Null, It.IsAny
        /// </summary>
        [Fact]
        public void GetMember_ShouldReturnNull_WhenNotExists()
        {

            _repo.Setup(r => r.GetById(It.IsAny<int>())).Returns((Member)null);


            var result = _service.GetMember(99);


            Assert.Null(result);
        }

        /// <summary>
        /// Тест перевіряє, що IsActive повертає true для активного користувача.
        /// Типи перевірок: Assert.True, Verify
        /// </summary>
        [Fact]
        public void IsActive_ShouldReturnTrue_WhenMemberIsActive()
        {

            var member = new Member { Id = 1, IsActive = true };
            _repo.Setup(r => r.GetById(1)).Returns(member);


            var result = _service.IsActive(1);


            Assert.True(result);
            _repo.Verify(r => r.GetById(1));
        }

        /// <summary>
        /// Тест перевіряє, що IsActive повертає false для неактивного користувача.
        /// Типи перевірок: Assert.False
        /// </summary>
        [Fact]
        public void IsActive_ShouldReturnFalse_WhenMemberIsNotActive()
        {

            var member = new Member { Id = 1, IsActive = false };
            _repo.Setup(r => r.GetById(1)).Returns(member);


            var result = _service.IsActive(1);


            Assert.False(result);
        }
    }
}