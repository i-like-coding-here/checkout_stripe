import { useEffect, useState } from "react";

const UpdatePlans = () => {
  const [prices, setPrices] = useState([]);
  const [error, setError] = useState("");

  const subscriptionId = "sub_1RRSauSAXnM1xqlcHRwMhGE5";
  // const NewPrice = "price_1RPIAhSAXnM1xqlcbZBuQsCc";

  useEffect(() => {
    const fetchConfig = async () => {
      try {
        const res = await fetch("/api/config");
        const data = await res.json();
        console.log("prices: ", data.prices);
        
        setPrices(data.prices || []);
      } catch (err) {
        console.error(err);
        setError("Failed to load pricing. Please try again.");
      }
    };

    fetchConfig();
  }, []);


  const handleUpdateSubscription = async (subscriptionId, NewPrice) => {
    try {
      const res = await fetch("/api/update-subscription", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          subscriptionId: subscriptionId,
          newPriceLookupKey: NewPrice
        }),
      });
      // console.log(body);
      

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
    <div>
      <div>
        <h1>Update your plan</h1>

        <div>
          {prices.map((price) => (
            <section key={price.id}>
              <div>{price.lookup_key || "Untitled Plan"}</div>
              <div>${(price.unit_amount / 100).toFixed(2)}</div>
              <div>per {price.recurring?.interval || "period"}</div>
              {/* <button onClick={() => handleCancel()}></button> */}
              <button onClick={() => handleUpdateSubscription(subscriptionId, price.id)}>Select</button>
            </section>
          ))}
        </div>


        {error && <div style={{ color: "red" }}>{error}</div>}
      </div>
    </div>
  );
};

export default UpdatePlans;