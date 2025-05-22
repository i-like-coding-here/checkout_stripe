import React from 'react'
import { Link } from 'react-router-dom';
import { useState, useEffect } from 'react';


const AccountSubscription = ({ subscription }) => {

const [error, setError] = useState(null)

    const handleCancelSubscription = async() => {
        try {
            const res = await fetch("/api/cancel-subscription", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    subscriptionId: subscription.id,
                }),
            });


            const result = await res.json();
            console.log("result: ", result);


            if (res.ok && result.url) {
                window.location.href = result.url;
            } else if (result.error) {
                setError(result.error);
            }
        } catch (err) {
            console.error(err);
            setError("Something failed. Please try again.");
        }
    };

    
        const handlePauseSubscription = async() => {
        try {
            const res = await fetch("/api/pause-subscription", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    subscriptionId: subscription.id,
                }),
            });


            const result = await res.json();
            console.log("result: ", result);


            if (res.ok && result.url) {
                window.location.href = result.url;
            } else if (result.error) {
                setError(result.error);
            }
        } catch (error) {
            console.error(error);
            setError("Something failed. Please try again.");
        }
    };

        const handleResumeSubscription = async() => {
        try {
            const res = await fetch("/api/resume-subscription", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    subscriptionId: subscription.id,
                }),
            });


            const result = await res.json();
            console.log("result: ", result);


            if (res.ok && result.url) {
                window.location.href = result.url;
            } else if (result.error) {
                setError(result.error);
            }
        } catch (err) {
            console.error(err);
            setError("Something failed. Please try again.");
        }
    };

    return (
        <section className="account-subscription">
            <hr />
            <h4>
                <a href={`https://dashboard.stripe.com/test/subscriptions/${subscription.id}`}>
                    {subscription.id}
                </a>
            </h4>
            {/* <p>Price Id: {subscription.}</p> */}
            <p>Status: {subscription.status}</p>
            <p>customerId: {subscription.customer}</p>
            <p>Card last4: {subscription.default_payment_method?.card?.last4}</p>
            <p>Current period end: {new Date(subscription.current_period_end * 1000).toString()}</p>
            <button onClick={() => handlePauseSubscription()}>Pause Subscription</button>
            <button onClick={() => handleCancelSubscription()}>Cancel Subscription</button>
            <button onClick={() => handleResumeSubscription()}>Resume Subscription</button>
            {/* <div className="account-actions">
                <Link to="/cancel" state={{ subscription: subscription.id }}>Cancel</Link>
            </div> */}
        </section>
    );
}

const ActionPlan = () => {

    const tenantId = "BDF56965-1EC8-4E2C-9AD4-911E15F43895"
    const subscriptionId = "sub_1RRAsfSAXnM1xqlcCFVj6g0D";
    const customerId = "cus_SMCCo7Si4WiZwO";
    const [subscriptions, setSubscriptions] = useState([]);

    useEffect(() => {
        const fetchData = async () => {
            const { subscriptions } = await fetch(`/api/subscriptions?tenantId=${tenantId}&customerId=${customerId}`)
                .then(r => r.json());
            console.log(subscriptions);

            setSubscriptions(subscriptions.data);
        };
        fetchData();
    }, [tenantId, customerId]);

    return (
        <>
            <Link to="/newplan">Add a subscription</Link>
            <hr></hr>
            <Link to="/updateplan">Update existing subscriptions</Link>
            <div id="subscriptions">
                {subscriptions.map(s => <AccountSubscription key={s.id} subscription={s} />)}
            </div>
        </>
    )
}

export default ActionPlan   