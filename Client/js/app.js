(function (THREE) {

    var scene = new THREE.Scene();
    var camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    var object;
    var renderer = new THREE.WebGLRenderer();
    renderer.setSize(window.innerWidth, window.innerHeight);
    document.body.appendChild(renderer.domElement);
    var mouse3D = position = { x: 1, y: 1, z: 1 };
    //var scalefactor = new THREE.Vector3(1,1,1);

    var projector = new THREE.Projector();

    var loader = new THREE.JSONLoader(true);
    loader.load('./assets/models/suzanne.js', function(geometry, materials) {
        object = new THREE.Mesh(geometry, new THREE.MeshPhongMaterial({ wireframe: false, ambient: 0x555555, color: 0xaaaaaa, specular: 0x00ffff, shininess: 20, shading: THREE.SmoothShading }));
        object.position.set(0,0,0);
        
        scene.add(object);
        render();
    });       

    
    camera.position.z = 2;

    var render = function () {
        requestAnimationFrame(render);
        camera.updateProjectionMatrix();
        object.position = mouse3D;
        //object.scale.set(scalefactor);
        renderer.render(scene, camera);
    };

    var ambientLight = new THREE.AmbientLight(0x004400);
    scene.add(ambientLight);

    // directional lighting
    var directionalLight = new THREE.DirectionalLight(0xffff00);
    directionalLight.position.set(3, 3, 3).normalize();
    directionalLight.intensity = 0.3;
    scene.add(directionalLight);

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

    var socket = new WebSocket('ws://10.1.0.165:8181');

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
        var posdata = JSON.parse(e.data);
        //console.log("Left hand: " + posdata.LeftHandJointPosition.X + ", " + posdata.LeftHandJointPosition.Y + ", " + posdata.LeftHandJointPosition.Z);
        //console.log("Right hand: " + posdata.RightHandJointPosition.X + ", " + posdata.RightHandJointPosition.Y + ", " + posdata.RightHandJointPosition.Z);
        /*
        if (posdata.lightsOn) {
            directionalLight.intensity = 0.3;
        }
        else {
            directionalLight.intensity = 0.0;
        }
        */


        //mouse3D = new THREE.Vector3(posdata.RightHandJointPosition.X, posdata.RightHandJointPosition.Y, posdata.RightHandJointPosition.Z); // tracking right hand position
        mouse3D = new THREE.Vector3(posdata.HeadJointPosition.X, posdata.HeadJointPosition.Y, 1); // tracking head position
        mouse3D.sub(camera.position);

        leftHandVector = new THREE.Vector3(0, 0, posdata.LeftHandJointPosition.Z);
        //scalefactor = mouse3D.sub(leftHandVector);
        scalefactor = leftHandVector;

        
        //mouse3D.normalize();
    };

})(THREE);