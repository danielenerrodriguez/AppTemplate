using AppTemplate.Api.Shared.Models;
using FluentAssertions;

namespace AppTemplate.Api.Tests.Shared.Models;

public class PagedResultTests
{
    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var items = new List<string> { "a", "b", "c" }.AsReadOnly();

        var result = PagedResult<string>.Create(items, totalCount: 10, page: 2, pageSize: 3);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(10);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(3);
    }

    [Fact]
    public void TotalPages_CalculatesCorrectly()
    {
        var result = PagedResult<string>.Create([], totalCount: 10, page: 1, pageSize: 3);

        result.TotalPages.Should().Be(4); // ceil(10/3) = 4
    }

    [Fact]
    public void HasNextPage_WhenNotLastPage_ReturnsTrue()
    {
        var result = PagedResult<string>.Create([], totalCount: 10, page: 2, pageSize: 3);

        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_WhenLastPage_ReturnsFalse()
    {
        var result = PagedResult<string>.Create([], totalCount: 10, page: 4, pageSize: 3);

        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_WhenFirstPage_ReturnsFalse()
    {
        var result = PagedResult<string>.Create([], totalCount: 10, page: 1, pageSize: 3);

        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_WhenNotFirstPage_ReturnsTrue()
    {
        var result = PagedResult<string>.Create([], totalCount: 10, page: 2, pageSize: 3);

        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void TotalPages_WhenPageSizeIsZero_ReturnsZero()
    {
        var result = PagedResult<string>.Create([], totalCount: 10, page: 1, pageSize: 0);

        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void Create_WhenTotalCountIsZero_HasCorrectPagination()
    {
        var result = PagedResult<string>.Create([], totalCount: 0, page: 1, pageSize: 10);

        result.TotalPages.Should().Be(0);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void TotalPages_WhenTotalCountEqualsPageSize_ReturnsOne()
    {
        var result = PagedResult<string>.Create(["a", "b", "c"], totalCount: 3, page: 1, pageSize: 3);

        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
    }
}
