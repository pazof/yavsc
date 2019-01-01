+(function($, paypal, PAYPAL_ENV, CREATE_PAYMENT_URL, EXECUTE_PAYMENT_URL) {
    $(document).ready(function() {

        paypal.Button.render({

            env: PAYPAL_ENV, // 'production', Optional: specify 'sandbox' environment
            commit: true,
            payment: function(resolve, reject) {

                return paypal.request.post(CREATE_PAYMENT_URL)
                    .then(function(data) { resolve(data.id); })
                    .catch(function(err) { reject(err); });
            },

            onAuthorize: function(data) {

                // Note: you can display a confirmation page before executing

                return paypal.request.post(EXECUTE_PAYMENT_URL, { paymentID: data.paymentID, payerID: data.payerID })

                .then(function(data) {
                        document.location = '@ViewBag.Urls.Details';
                        /* Go to a success page */
                    })
                    .catch(function(err) {
                        document.location = '/Manage/PaymentInfo/' + data.paymentID + '/?error=' + err;
                        /* Go to an error page  */
                    });
            }

        }, '#paypal-button');
    })
})(jQuery);