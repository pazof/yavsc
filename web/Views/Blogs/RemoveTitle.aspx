<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntryCollection>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<%= Html.ValidationSummary() %>
<% using (Html.BeginForm("RemoveTitle","Blogs")) { %>

<%= Html.LabelFor(model => model.Titles) %> :
<%= Html.TextBox( "Titles" ) %>
<%= Html.ValidationMessage("Titles", "*") %><br/>
<%= Html.Hidden("UserName") %>
<label for="confirm">supprimer le billet</label>
<%= Html.CheckBox( "confirm" ) %>
<%= Html.ValidationMessage("AgreeToRemove", "*") %>

<%= Html.Hidden("returnUrl") %>
 <input type="submit"/>
 <% } %>

</asp:Content>


