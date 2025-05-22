const CheckoutCancelled = () => {
  return (
    <div>
      <div>
        <div>
          <header>
            <div></div>
          </header>
          <div>
            <h1>Your payment was canceled</h1>
            <button onClick={() => (window.location.href = "/")}>
              Restart demo
            </button>
          </div>
        </div>
        <div></div>
      </div>
    </div>
  );
};

export default CheckoutCancelled;
