(function (THREE) {

    var lightsOn = false;
    var scene = new THREE.Scene();
    var camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);

    var renderer = new THREE.WebGLRenderer();
    renderer.setSize(window.innerWidth, window.innerHeight);
    document.body.appendChild(renderer.domElement);
    var mouse3D = { x: 1, y: 1, z: 1 };

    var geometry = new THREE.CubeGeometry(1, 1, 1);
    //var material = new THREE.MeshBasicMaterial({wireframe: false, color: 0xffffff});
    var material = new THREE.MeshPhongMaterial({ wireframe: false, ambient: 0x555555, color: 0xaaaaaa, specular: 0x00ffff, shininess: 20, shading: THREE.SmoothShading });
    var cube = new THREE.Mesh(geometry, material);
    var projector = new THREE.Projector();

    scene.add(cube);
    camera.position.z = 1;

    var render = function () {
        requestAnimationFrame(render);
        camera.updateProjectionMatrix();

        cube.position = mouse3D;
        renderer.render(scene, camera);
    };

    var ambientLight = new THREE.AmbientLight(0x004400);
    scene.add(ambientLight);

    // directional lighting
    var directionalLight = new THREE.DirectionalLight(0xffff00);
    directionalLight.position.set(3, 3, 3).normalize();
    directionalLight.intensity = 0.3;
    scene.add(directionalLight);

    render();

    // Events
    //document.addEventListener('mousemove', mousemove, false);

    function mousemove(event) {
        mouse3D = new THREE.Vector3((event.clientX / window.innerWidth) * 2 - 1,   //x
                                        -(event.clientY / window.innerHeight) * 2 + 1,  //y
                                        0.5);                                            //z
        projector.unprojectVector(mouse3D, camera);
        mouse3D.sub(camera.position);
        mouse3D.normalize();

    };

    var socket = new WebSocket('ws://10.1.0.201:8181');

    // When the connection is open, send some data to the server
    socket.onopen = function () {
        socket.send('Helloooo'); // Send the message 'Ping' to the server
    };

    // Log errors
    socket.onerror = function (error) {
        console.log('WebSocket Error ' + error);
    };

    // Log messages from the server
    socket.onmessage = function (e) {
        console.log('Server: ' + e.data);

        var posdata = JSON.parse(e.data);
        console.log("data: " + posdata);

        if (posdata.lightsOn) {
            directionalLight.intensity = 0.3;
        }
        else {
            directionalLight.intensity = 0.0;
        }

        mouse3D = new THREE.Vector3((posdata.x / window.innerWidth) * 2 - 1,   //x
                                        -(posdata.y / window.innerHeight) * 2 + 1,  //y
                                        0.5);                                            //z
        projector.unprojectVector(mouse3D, camera);
        mouse3D.sub(camera.position);
        mouse3D.normalize();
    };

})(THREE);