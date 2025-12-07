<template>
  <div id="app">
    <CoffeeMachine />
  </div>
</template>

<script>
import CoffeeMachine from './components/CoffeeMachine.vue'

export default {
  name: 'App',
  components: {
    CoffeeMachine
  }
}
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  min-height: 100vh;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

#app {
  min-height: 100vh;
  padding: 20px;
}
</style>