import { useEffect, useState } from "react";

const AccountSubscription = ({ subscription }) => {
    return (
        <section className="account-subscription">
            <hr />
            <h4>
                <a href={`https://dashboard.stripe.com/test/subscriptions/${subscription.id}`}>
                    {subscription.id}
                </a>
            </h4>
            <p>Status: {subscription.status}</p>
            <p>customerId: {subscription.customer}</p>
            <p>Card last4: {subscription.default_payment_method?.card?.last4}</p>
            <p>Current period end: {new Date(subscription.current_period_end * 1000).toString()}</p>
            {/* <div className="account-actions">
                <Link to="/cancel" state={{ subscription: subscription.id }}>Cancel</Link>
            </div> */}
        </section>
    );
};

const CheckoutSuccess = () => {
    const [session, setSession] = useState(null);
    const [sessionId, setSessionId] = useState("");
    const [subscriptionId, setSubscriptionId] = useState("sub_1RRAsfSAXnM1xqlcCFVj6g0D");
    const NewPrice= "price_1RPIAiSAXnM1xqlcA9Dsj3TT";
    const [subscriptions, setSubscriptions] = useState([]);
    const tenantId = "69DBFAFA-667E-4344-A141-DE8B6B9810ED"
    const customerId = "cus_SLsdPJHXZsH3cO";

    useEffect(() => {
        const fetchSession = async () => {
            const urlParams = new URLSearchParams(window.location.search);
            const id = urlParams.get("session_id");

            if (id) {
                setSessionId(id);

                try {
                    const response = await fetch(`/api/checkout-session?sessionId=${id}`);
                    const data = await response.json();
                    setSession(data);
                } catch (err) {
                    console.error("Failed to fetch checkout session:", err);
                }
            }
        };

        fetchSession();
    }, []);

    useEffect(() => {
        const fetchData = async () => {
            const { subscriptions } = await fetch(`/api/subscriptions?tenantId=${tenantId}&customerId=${customerId}`)
                .then(r => r.json());
            setSubscriptions(subscriptions.data);
        };
        fetchData();
    }, [tenantId, customerId]);

    if (!subscriptions) return null;

    const handleUpdateSubscription = async (Subscription, NewPrice) => {
        try {
            const res = await fetch("/api/update-subscription", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                     subscriptionId: Subscription,
                     newPriceLookupKey: NewPrice 
                    }),
            });

            const result = await res.json();

            setSubscriptionId(result.SubscriptionId);

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
        <div>
            <div>
                <div>
                    <div>
                        <h1>Your payment succeeded</h1>
                        <h4>View CheckoutSession response:</h4>
                    </div>

                    <div>
                        <pre>{session ? JSON.stringify(session, null, 2) : "Loading..."}</pre>

                        <button onClick={() => (window.location.href = "/")}>
                            Restart demo
                        </button>
                        <button onClick={() => handleUpdateSubscription(subscriptionId, NewPrice)}>Update Subscription</button>

                        <form action="/api/customer-portal" method="POST">
                            <input type="hidden" name="sessionId" value={sessionId} />
                            <button>Manage Billing</button>
                        </form>
                    </div>
                    <h2>Current Subscriptions</h2>
                    <div id="subscriptions">
                        {subscriptions.map(s => <AccountSubscription key={s.id} subscription={s} />)}
                    </div>
                    <div></div>
                </div>
            </div>
        </div>
    );
};

export default CheckoutSuccess;
