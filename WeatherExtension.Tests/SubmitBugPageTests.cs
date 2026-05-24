// Copyright (c) Bald Bearded Builder LLC
// Bald Bearded Builder LLC licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BaldBeardedBuilder.WeatherExtension;
using Microsoft.CmdPal.Ext.Weather.Pages;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Microsoft.CmdPal.Ext.Weather.UnitTests;

/// <summary>
/// Tests for <see cref="SubmitBugPage"/>.
/// Verifies metadata, item count, and that each item carries a Details panel.
/// Follows the same pattern as <see cref="EmptySearchHintPageTests"/>.
/// </summary>
[TestClass]
public class SubmitBugPageTests
{
	// ---------------------------------------------------------------
	// Metadata
	// ---------------------------------------------------------------

	[TestMethod]
	public void Name_IsBugReportTitle()
	{
		var page = new SubmitBugPage();

		Assert.AreEqual(Resources.bug_report_title, page.Name);
	}

	[TestMethod]
	public void Title_IsBugReportTitle()
	{
		var page = new SubmitBugPage();

		Assert.AreEqual(Resources.bug_report_title, page.Title);
	}

	[TestMethod]
	public void Icon_IsNotNull()
	{
		var page = new SubmitBugPage();

		Assert.IsNotNull(page.Icon);
	}

	[TestMethod]
	public void Id_IsNotNullOrEmpty()
	{
		var page = new SubmitBugPage();

		Assert.IsFalse(string.IsNullOrWhiteSpace(page.Id),
			"Page Id must be set so CmdPal can uniquely identify the page.");
	}

	[TestMethod]
	public void ShowDetails_IsTrue()
	{
		var page = new SubmitBugPage();

		Assert.IsTrue(page.ShowDetails,
			"ShowDetails must be true so the instructions Details panel is visible.");
	}

	// ---------------------------------------------------------------
	// GetItems() — count and structure
	// ---------------------------------------------------------------

	[TestMethod]
	public void GetItems_ReturnsExactlyTwoItems()
	{
		var page = new SubmitBugPage();

		var items = page.GetItems();

		Assert.AreEqual(2, items.Length,
			"SubmitBugPage must return exactly 2 items: Save Logs and Open GitHub Issues.");
	}

	[TestMethod]
	public void GetItems_FirstItem_HasSaveLogsTitle()
	{
		var page = new SubmitBugPage();

		var firstItem = (ListItem)page.GetItems()[0];

		Assert.AreEqual(Resources.bug_report_save_logs, firstItem.Title,
			"First item must be the Save Logs action.");
	}

	[TestMethod]
	public void GetItems_SecondItem_HasOpenGitHubTitle()
	{
		var page = new SubmitBugPage();

		var secondItem = (ListItem)page.GetItems()[1];

		Assert.AreEqual(Resources.bug_report_open_github, secondItem.Title,
			"Second item must be the Open GitHub Issues action.");
	}

	[TestMethod]
	public void GetItems_BothItems_HaveDetailsPanel()
	{
		var page = new SubmitBugPage();

		var items = page.GetItems();

		foreach (var item in items.Cast<ListItem>())
		{
			Assert.IsNotNull(item.Details,
				$"Item '{item.Title}' must have a Details panel with instructions.");
		}
	}

	[TestMethod]
	public void GetItems_DetailsPanel_HasNonEmptyBody()
	{
		var page = new SubmitBugPage();

		var firstItem = (ListItem)page.GetItems()[0];
		var details = (Details)firstItem.Details!;

		Assert.IsFalse(string.IsNullOrWhiteSpace(details.Body),
			"Details.Body must contain the bug report instructions and not be empty.");
	}

	[TestMethod]
	public void GetItems_DetailsPanel_TitleIsBugReportTitle()
	{
		var page = new SubmitBugPage();

		var firstItem = (ListItem)page.GetItems()[0];
		var details = (Details)firstItem.Details!;

		Assert.AreEqual(Resources.bug_report_title, details.Title,
			"Details.Title must match the page title for consistent header display.");
	}

	// ---------------------------------------------------------------
	// Stability
	// ---------------------------------------------------------------

	[TestMethod]
	public void GetItems_CalledTwice_ReturnsSameInstances()
	{
		var page = new SubmitBugPage();

		var items1 = page.GetItems();
		var items2 = page.GetItems();

		Assert.AreSame(items1[0], items2[0],
			"GetItems() should return the same pre-built array — no allocation on repeat calls.");
		Assert.AreSame(items1[1], items2[1],
			"GetItems() should return the same pre-built array — no allocation on repeat calls.");
	}
}
