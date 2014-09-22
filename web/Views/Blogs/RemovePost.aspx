<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntry>" MasterPageFile="~/Models/App.master" %>


<asp:Content ContentPlaceHolderID="titleContent" ID="titleContentContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<form runat="server">
<%= Html.ValidationSummary() %>
<% using (Html.BeginForm("Remove","Blogs")) { %>
 suivant : 
<%= Html.LabelFor(model => model.Title) %>:<br/>
<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %>
<label for="confirm">supprimer le billet</label>
<input type="checkbox" name="confirm" />
<%= Html.ValidationMessage("AgreeToRemove", "*") %>

 <input type="submit"/>
 <% } %>
 </form>
</asp:Content>


