<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEditEntryModel>" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="head" ID="head" runat="server">
	<title><%= Html.Encode(ViewData["BlogTitle"]) %>  edition 
	- <%=Html.Encode(YavscHelpers.SiteName) %>
	</title>
</asp:Content>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">

<h1 class="blogtitle"> 
	<a href="/Blog/<%=ViewData["UserName"]%>">
 <img class="avatar" src="/Blogs/Avatar?user=<%=ViewData["UserName"]%>" alt="from <%=ViewData["UserName"]%>"/>
 <%= Html.Encode(ViewData["BlogTitle"]) %> </a> - 
	<%= Html.Encode(Model.Title) %> - 
	 &Eacute;dition </h1>

 
 
<div class="message">
	<%= Html.Encode(ViewData["Message"]) %>
</div>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">


<% if (Model != null ) if (Model.Content != null )  {
	 BBCodeHelper.Init (); %>
	 <%= Html.ActionLink(Model.Title,"UserPost",new{user=Model.UserName,title=Model.Title}) %>
<div class="blogpost">
<%= BBCodeHelper.Parser.ToHtml(Model.Content) %>
</div>
<% } %>
Usage BBcodes  :
	 <div style="font-family:monospace;">
<%
	foreach (string usage in BBCodeHelper.BBTagsUsage) { %>
<div style="display:inline"><%= usage %></div>
<% } %>
	</div>

<%= Html.ValidationSummary("Edition du billet") %>

<% using(Html.BeginForm("ValidateEdit", "Blogs")) { %>
<%= Html.LabelFor(model => model.Title) %>:<br/>
<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %>
<br/>
<%= Html.LabelFor(model => model.Content) %>:<br/>
<%= Html.TextArea( "Content" , new { @class="editblog", @rows="15" }) %>
<%= Html.ValidationMessage("Content", "*") %>
<br/>
<%= Html.CheckBox( "Visible" ) %>
<%= Html.LabelFor(model => model.Visible) %>
<%= Html.ValidationMessage("Visible", "*") %>
	<br/>
<%= Html.CheckBox( "Preview" ) %>
<%= Html.LabelFor(model => model.Preview) %>
<%= Html.ValidationMessage("Preview", "*") %>
	<%= Html.Hidden("Id") %>
	<%= Html.Hidden("UserName") %>

<br/>
<input type="submit"/>
<% } %>
</div>

</asp:Content>

