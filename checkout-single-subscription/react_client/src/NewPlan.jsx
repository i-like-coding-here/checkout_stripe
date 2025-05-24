import { useEffect, useState } from "react";

const NewPlan = ({ currentSubscriptions = [] }) => {
  const [prices, setPrices] = useState([]);
  const [selectedPrices, setSelectedPrices] = useState([]);
  const [error, setError] = useState("");

  // Extract subscribed price IDs from current subscriptions
  const subscribedPriceIds = currentSubscriptions.map((sub) => sub.priceId);

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

  const isEnterpriseSelected = selectedPrices.some(
    (p) => p.lookup_key === "enterprise"
  );

  const toggleSelectPrice = (price) => {
    const isSelected = selectedPrices.some((p) => p.id === price.id);

    if (isSelected) {
      // Deselect the plan
      setSelectedPrices(selectedPrices.filter((p) => p.id !== price.id));
    } else {
      if (price.lookup_key === "enterprise") {
        // If Enterprise selected, clear all others and select only Enterprise
        setSelectedPrices([price]);
      } else {
        // If selecting helpdesk or project, only allow if Enterprise NOT selected
        if (isEnterpriseSelected) {
          alert(
            "Enterprise plan already selected. Deselect it to choose other plans."
          );
          return;
        }
        setSelectedPrices([...selectedPrices, price]);
      }
    }
  };

  const handleCheckout = async () => {
    if (selectedPrices.length === 0) {
      setError("Please select at least one plan.");
      return;
    }

    setError("");

    try {
      // For demo: send only first selected plan to checkout
      // Adjust to batch process if your backend supports multiple prices at once
      const price = selectedPrices[0];
      console.log("Selected price object:", price);


      const res = await fetch("/api/create-checkout-session", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ priceId: price.id }),
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
      <h1>Choose a plan</h1>
      <div>
        {prices.map((price) => {
          const isSelected = selectedPrices.some((p) => p.id === price.id);
          const isSubscribed = subscribedPriceIds.includes(price.id);
          const isDisabled =
            (price.lookup_key !== "enterprise" && isEnterpriseSelected) ||
            isSubscribed;

          return (
            <section
              key={price.id}
              style={{
                border: isSelected ? "2px solid green" : "1px solid gray",
                opacity: isDisabled ? 0.5 : 1,
                pointerEvents: isDisabled ? "none" : "auto",
                marginBottom: "1rem",
                padding: "0.5rem",
                cursor: isDisabled ? "not-allowed" : "pointer",
              }}
              onClick={() => !isDisabled && toggleSelectPrice(price)}
            >
              <div>
                <strong>{price.lookup_key || "Untitled Plan"}</strong>
              </div>
              <div>${(price.unit_amount / 100).toFixed(2)}</div>
              <div>per {price.recurring?.interval || "period"}</div>
              <button
                disabled={isDisabled}
                onClick={(e) => {
                  e.stopPropagation();
                  toggleSelectPrice(price);
                }}
              >
                {isSelected ? "Deselect" : "Select"}
              </button>
              {isSubscribed && <small> (Already subscribed)</small>}
              {price.lookup_key === "enterprise"}
            </section>
          );
        })}
      </div>

      <button
        onClick={handleCheckout}
        style={{ marginTop: "1rem" }}
        disabled={selectedPrices.length === 0}
      >
        Subscribe / Upgrade
      </button>

      {error && <div style={{ color: "red", marginTop: "1rem" }}>{error}</div>}
    </div>
  );
};

export default NewPlan;
