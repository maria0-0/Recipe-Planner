using SharpQuest.Contracts;

namespace Capstone.Tests;

public class SearchMatcherTests
{
    private readonly SearchMatcher _matcher = SearchMatcher.Create();

   
    [Fact]
    public void Empty_or_whitespace_query_matches_any_non_null_text()
    {
        Assert.True(_matcher.Matches("Pasta Carbonara", ""));
        Assert.True(_matcher.Matches("Pasta Carbonara", "   "));
    }

    
    [Fact]
    public void Matching_ignores_case()
    {
        Assert.True(_matcher.Matches("Pizza Margherita", "PIZZA"));
        Assert.True(_matcher.Matches("Spaghetti Bolognese", "spaghetti"));
    }

    
    [Fact]
    public void Matching_ignores_diacritics_both_ways()
    {
        Assert.True(_matcher.Matches("Crème Brûlée", "creme brulee"));
        Assert.True(_matcher.Matches("creme brulee", "crème brûlée"));
        
    }

    
    [Fact]
    public void Every_query_word_must_be_present()
    {
        Assert.False(_matcher.Matches("Tomato Soup", "tomato basil"));
    }

    
    [Fact]
    public void Word_order_does_not_matter()
    {
        Assert.True(_matcher.Matches("Garlic Tomato Pasta", "tomato garlic"));
        Assert.True(_matcher.Matches("Tomato and Basil Pasta", "basil tomato"));
    }

    
    [Fact]
    public void Word_can_occur_inside_another_word()
    {
        Assert.True(_matcher.Matches("Spaghetti", "ghetti"));
        Assert.True(_matcher.Matches("Pepperoni", "roni"));

    }

    
    [Fact]
    public void Null_text_returns_false()
    {
        Assert.False(_matcher.Matches(null, "pasta"));
    }

    
    [Fact]
    public void Null_query_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _matcher.Matches("pasta", null!));
    }
}

