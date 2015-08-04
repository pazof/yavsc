<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntry>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<%= Html.ValidationSummary() %>
<% using (Html.BeginForm("RemovePost","Blogs",new {id=Model.Id})) { %>

<%= Html.LabelFor(model => model.Title) %> :
<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %><br/>
Identifiant du post : <%= Model.Id %><br/>
<span class="preview"><%= Html.Markdown(Model.Content) %></span>
<label for="confirm">supprimer le billet</label>
<%= Html.CheckBox( "confirm" ) %>
<%= Html.ValidationMessage("confirm", "*") %>
<%= Html.Hidden("returnUrl") %>
 <input type="submit"/>
 <% } %>

</asp:Content>
