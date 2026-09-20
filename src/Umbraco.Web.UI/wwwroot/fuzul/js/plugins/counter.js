// Get the elements we need to manipulate
const countElements = document.querySelectorAll('.count');

// Define the function that will update the count for each fact
function updateCount() {
  countElements.forEach(countEl => {
    const target = +countEl.getAttribute('data-target');
    let count = +countEl.textContent;
    const increment = target / 30; // Increase the count by 1% of the target value
    if (count < target) {
      count += increment;
      countEl.textContent = Math.floor(count); // Round down to the nearest integer
    } else {
      countEl.textContent = target;
    }
  });
}

// Set up the counter to start on page load
window.addEventListener('load', () => {
  updateCount();
});

// Set up the counter to run every 50ms
setInterval(updateCount, 30);
