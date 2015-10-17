<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogEntry>" %>

<ul id="tags">
<% if (Model.Tags != null) foreach (string tagname in Model.Tags) { %>
<li><%= tagname %></li>
<% } %>
</ul>
<% if (Membership.GetUser()!=null) { %>
<% if (Membership.GetUser().UserName==Model.Author || Roles.IsUserInRole("Admin"))
{ // grant all permissions: to choose a given set of tags, also create some new tags %>

<span id="viewtagger">
<i class="fa fa-tag menuitem" id="viewtaggerbtn"><%=Html.Translate("DoTag")%></i></span>
<span id="hidetagger" class="hidden">
<i class="fa fa-tag menuitem"  id="hidetaggerbtn" ><%=Html.Translate("Hide")%></i>
Note: Ils sont utilisé pour classifier le document. Par exemple, le tag <code>Accueil</code> rend le document 
éligible à une place en page d'Accueil.
</span>
<form id="tagger" class="hidden">
<fieldset>
<legend>Associer des tags au billet</legend>
 <label for="newtag"><%= Html.Translate("Tag_name")%>: </label>
 <span id="Err_tag" class="error"></span>
<input type="text" id="newtag">
<span id="Err_model" class="error"></span>
<input id="sendnewtag" type="submit" class="fa fa-tag" value="<%=Html.Translate("Submit")%>">
</fieldset>
</form>
<script>



$(document).ready(function(){

$('#hidetaggerbtn').click(function(){
$('#tagger').addClass('hidden');
$('#viewtagger').removeClass('hidden');
$('#hidetagger').addClass('hidden');
});
$('#viewtaggerbtn').click(function(){
$('#tagger').removeClass('hidden');
$('#viewtagger').addClass('hidden');
$('#hidetagger').removeClass('hidden');
});

	$('#newtag').autocomplete({
  	  minLength: 0,
      delay: 200,
      source:  function( request, response ) {
        $.ajax({
          url: "/api/Blogs/Tags",
          type: "POST",
          data: {
            pattern: request.term
          },
          success: function( data ) {
            response( data );
          }
        });
      },      
      select: function( event, ui ) {
        console.log( ui.item ?
          "Selected: " + ui.item.label :
          "Nothing selected, input was " + this.value);
      },
      open: function() {
        $( this ).removeClass( "ui-corner-all" ).addClass( "ui-corner-top" );
      },
      close: function() {
        $( this ).removeClass( "ui-corner-top" ).addClass( "ui-corner-all" );
      }
	});	
	$('#tagger').on('submit', function(e) { e.preventDefault(); });

	$('#sendnewtag').click(function(){
	var data = {
           postid: <%= Model.Id %>,
           tag: $('#newtag').val()
          }
	$.ajax({
          url: '/api/Blogs/Tag/',
          type: 'POST',
          data: data,
          success: function() {
            $('<li>'+data.tag+'</li>').appendTo('#tags');
            $('#newtag').val('');
          },
          statusCode: {
            	400: Yavsc.onAjaxBadInput
            	},
            error: Yavsc.onAjaxError
        });
	});
});
</script>
<% } %>
<% } %>
