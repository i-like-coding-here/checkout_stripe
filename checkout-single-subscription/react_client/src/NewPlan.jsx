import { useEffect, useState } from "react";

const NewPlan = () => {
  const [prices, setPrices] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchConfig = async () => {
      try {
        const res = await fetch("/api/config");
        const data = await res.json();
        setPrices(data.prices || []);
      } catch (err) {
        console.error(err);
        setError("Failed to load pricing. Please try again.");
      }
    };

    fetchConfig();
  }, []);

  const handleCheckout = async (priceId) => {
    try {
      const res = await fetch("/api/create-checkout-session", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ priceId }),
      });

      const result = await res.json();

      if (res.ok && result.url) {
        window.location.href = result.url;
      } else if (result.error) {
        setError(result.error);
      }
    } catch (err) {
      console.error(err);
      setError("Checkout failed. Please try again.");
    }
  };

  return (
    <div>
      <div>
        <h1>Choose a plan</h1>

        <div>
          {prices.map((price) => (
            <section key={price.id}>
              <div>{price.lookup_key || "Untitled Plan"}</div>
              <div>${(price.unit_amount / 100).toFixed(2)}</div>
              <div>per {price.recurring?.interval || "period"}</div>
              <button onClick={() => handleCheckout(price.id)}>Select</button>
            </section>
          ))}
        </div>
        

        {error && <div style={{ color: "red" }}>{error}</div>}
      </div>
    </div>
  );
};

export default NewPlan;
