(function() {
  (function(jQuery) {
    return jQuery.widget('Yavsc.user', {
    options: {
      disabled: false,
      visiter: null,
      circles: [],
      },
    user:null,
    buttonCircles: null,
    buttonBlog: null,
    buttonInstMsg: null,
    buttonMailling: null,
    buttonAdmin: null,
    circles: [],
    _create: function() {
        var _this = this;
        var $this = $(this);
	    var $ctl = $(this.element); 
	    if (!_this.options.disabled) {
            _this.user = $ctl.data('user');
		    var roles = $ctl.data('roles');
		    var bcounter = $ctl.data('blog-counter');
		    var circlesspec = $ctl.data('circles');
		    if (bcounter)
	        if (bcounter>0) {
	   	 		_this.buttonBlog = $(' <a class="link"><i class="fa fa-folder"></i></a>').
	   	 		attr('href','/blog/'+_this.user);
	   	 		$ctl.append(_this.buttonBlog);
	   	 	}

		   	if (circlesspec)
		   	{
		   	    _this.circles = circlesspec.split(' ');
		   	}
		   	// Builds the inner controls

		   	var $fieldset = $('<fieldset></fieldset>');
		   	var $select  = $("<span></span>") ;
	   	    for (i = 0; i < _this.options.circles.length; i++) {
	   	        var checked = _this.circles.indexOf(_this.options.circles[i])>-1;
	   	        checked =  (checked) ? " checked" : "";
    			var $opt = $( "<input type='checkbox' id='c"+i+"' name='"+
    			_this.options.circles[i]+"' "+checked+"><label for='c"+i+"'>"+
    			 _this.options.circles[i] + "</label>")
    			.click(function(){
    			  if (this.checked) Yavsc.ajax("Account/AddUserToCircle",
    			  {UserName:_this.user,Circle:$(this).attr('name')});
    			  else Yavsc.ajax("Account/RemoveUserFromCircle",
    			  {UserName:_this.user,Circle:$(this).attr('name')});
    			}).appendTo($select);
			}
			var $form = $( '<form></form>' );
			$('<i></i>').addClass('fa').addClass('fa-users')
			 .appendTo($fieldset)
			 .appendTo ($form);
			 $select.appendTo ($form);
			_this.buttonCircles = $form;
			$ctl.append(_this.buttonCircles);
			return  $ctl;
	    }
	},
})})(jQuery);
}).call(this);

