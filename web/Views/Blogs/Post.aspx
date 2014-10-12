<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEditEntryModel>" MasterPageFile="~/Models/App.master"%>

<asp:Content ContentPlaceHolderID="overHeaderOne" ID="headerContent" runat="server">	
<h1 class="blogtitle"> 
	<a href="/Blog/<%=ViewData["UserName"]%>">
 <img class="avatar" src="/Blogs/Avatar?user=<%=ViewData["UserName"]%>" alt="from <%=ViewData["UserName"]%>"/>
 <%= Html.Encode(ViewData["BlogTitle"]) %> </a> - 
Nouveau billet -
<a href="/"><%=  Html.Encode(YavscHelpers.SiteName) %></a> </h1>
 
<div class="message">
	<%= Html.Encode(ViewData["Message"]) %>
</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div class="message">
	<%= Html.Encode(ViewData["Message"]) %>
</div>
	<% if (Model != null ) if (Model.Content != null )  { %>
<div class="blogpost">
<%= BBCodeHelper.Parser.ToHtml(Model.Content) %>
</div>
<% } %>
	<div class="editpost">


<%= Html.ValidationSummary() %>
<% using(Html.BeginForm("ValidatePost", "Blogs")) %>
<% { %>

<%= Html.LabelFor(model => model.Title) %>:<br/>
<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %>
<br/>
<div>
	Usage BBcodes  :
	 <ul>
<%
	foreach (string usage in BBCodeHelper.BBTagsUsage) { %>
<li><%= usage %></li>
<% } %>
	</ul>

</div>
<%= Html.LabelFor(model => model.Content) %>:<br/>
<%= Html.TextArea( "Content", new { @class="editblog", @rows="15" }) %>
<%= Html.ValidationMessage("Content", "*") %>
<br/>
<%= Html.CheckBox( "Visible" ) %>
<%= Html.LabelFor(model => model.Visible) %>
<%= Html.ValidationMessage("Visible", "*") %>
<br/>
<%= Html.CheckBox( "Preview" ) %> <%= Html.LabelFor(model => model.Preview) %>
<%= Html.ValidationMessage("Preview", "*") %>
<br/>
	<%= Html.Hidden("Id") %>
<input type="submit"/>
<% } %>

</div>
</asp:Content>

