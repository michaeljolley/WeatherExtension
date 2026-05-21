// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BaldBeardedBuilder.WeatherExtension;
using Microsoft.CmdPal.Ext.Weather.Pages;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

[TestClass]
public class SearchHintPageTests
{
	[TestMethod]
	public void GetContent_ReturnsMarkdownWithHeadlineAndHints()
	{
		var page = new SearchHintPage(Resources.no_favorites_hint);

		var content = page.GetContent();

		Assert.AreEqual(1, content.Length);
		var markdown = (MarkdownContent)content[0];
		StringAssert.Contains(markdown.Body, Resources.no_favorites_hint);
		StringAssert.Contains(markdown.Body, Resources.search_hint_examples_title);
		var firstExample = Resources.search_hint_examples_block
			.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
		StringAssert.Contains(markdown.Body, firstExample);
	}

	[TestMethod]
	public void GetContent_WithSearchFormat_IncludesFormatHint()
	{
		var page = new SearchHintPage(Resources.no_locations_found, includeSearchFormatHint: true);

		var markdown = (MarkdownContent)page.GetContent()[0];

		StringAssert.Contains(markdown.Body, Resources.search_format_hint);
		StringAssert.Contains(markdown.Body, Resources.search_hint_multiple_favorites);
	}
}
