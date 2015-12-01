(function() {
  (function(jQuery) {
    return jQuery.widget('Yavsc.hidefieldset', {
    options: {
      jsCallBack: null,
      disabled: false
      },
    button: null,
    _create: function() {
	   var $ctl = $(this.element); 
	   var _this = this;
	   var _btn = $ctl.children('legend');
	   if (!this.options.disabled && _btn) {
	        _btn.addClass('actionlink');
	        _btn.addClass('fa');
	        _btn.addClass('fa-eye');
	        this.button = _btn;
	        $ctl.children(':not(legend)').hide();
	   		this.button.click( function (e) {
	   		if ( _btn.hasClass('fa-eye') )  {
	        _btn.removeClass('fa-eye');
	        _btn.addClass('fa-eye-slash');
	   		     
		   		$ctl.children(':not(legend)').show();
		   		}
		   		else {
	        _btn.addClass('fa-eye');
	        _btn.removeClass('fa-eye-slash');
	   		     
		   		$ctl.children(':not(legend)').hide();

		   		}
	   		});
	   }
	},
})})(jQuery);
}).call(this);

$(document).ready(function(){
$('fieldset').hidefieldset();
});

