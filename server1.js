const express = require('express');
const cors = require('cors');
const nodemailer = require('nodemailer');
const { Sequelize, DataTypes } = require('sequelize');

// Create an instance of the express app
const app = express();
const port = 3001;

// Database setup
const sequelize = new Sequelize('sqlite:app.db');

// Define the MakeupBooking model
const MakeupBooking = sequelize.define('MakeupBooking', {
    name: {
        type: DataTypes.STRING,
        allowNull: false
    },
    mobile: {
        type: DataTypes.STRING,
        allowNull: false
    },
    recipientEmail: {
        type: DataTypes.STRING,
        allowNull: false
    },
    eventDate: {
        type: DataTypes.DATE,
        allowNull: false
    },
    makeupType: {
        type: DataTypes.STRING,
        allowNull: false
    },
    numberOfMakeups: {
        type: DataTypes.INTEGER,
        allowNull: false
    },
    readyTime: {
        type: DataTypes.STRING, // Corrected type
        allowNull: false
    },
    eventLocation: {
        type: DataTypes.STRING,
        allowNull: false
    }
}, {
    timestamps: true // Enable timestamps to include createdAt and updatedAt
});

// Create a nodemailer transporter
const transporter = nodemailer.createTransport({
    service: 'gmail',
    port: 587,
    secure: false,
    auth: {
        user: 'bakchodikarni@gmail.com', // Replace with your Gmail email address
        pass: 'dglj qntu gvns vlto' // Replace with your Gmail App Password
    }
});

// Middleware to parse JSON requests
app.use(express.json());
app.use(cors()); // Enable CORS for all routes

// Helper function to stringify objects
function stringifyObject(obj) {
    if (typeof obj === 'object') {
        if (Array.isArray(obj)) {
            return obj.map(item => stringifyObject(item)).join(', ');
        } else if (obj !== null) {
            return JSON.stringify(obj, null, 2); // 2 is the number of spaces for indentation
        }
    }
    return obj;
}

// Endpoint for form submission
app.post('/submit', async (req, res) => {
    console.log('Received form data:', req.body);

    try {
        // Extract form data from the request
        const { name, mobile, recipientEmail, eventDate, makeupType, numberOfMakeups, readyTime, eventLocation } = req.body;

        // Log the data being saved to the database
        console.log('Saving booking to database:', req.body);

        // Save booking to database
        const booking = await MakeupBooking.create({
            name,
            mobile,
            recipientEmail,
            eventDate,
            makeupType,
            numberOfMakeups,
            readyTime,
            eventLocation
        });

        console.log('Booking saved to database:', booking);

        // Sending confirmation email to the user
        const userMailOptions = {
            from: 'bakchodikarni@gmail.com',
            to: recipientEmail,
            subject: 'Confirmation Email',
            text: 'Thank you for submitting the form. We will get back to you soon.'
        };

        // Sending confirmation email to admin
        const adminMailOptions = {
            from: 'bakchodikarni@gmail.com',
            to: 'ishitagulati15@gmail.com',
            subject: 'New Form Submission',
            text: `New form submission:\n` +
                `Name: ${stringifyObject(name)}\n` +
                `Mobile: ${stringifyObject(mobile)}\n` +
                `Email: ${recipientEmail}\n` +
                `Event Date: ${stringifyObject(eventDate)}\n` +
                `Makeup Type: ${stringifyObject(makeupType)}\n` +
                `Number of Makeups: ${stringifyObject(numberOfMakeups)}\n` +
                `Ready Time: ${stringifyObject(readyTime)}\n` +
                `Location: ${stringifyObject(eventLocation)}`
        };

        // Send confirmation emails
        await transporter.sendMail(userMailOptions);
        console.log('User confirmation email sent');

        await transporter.sendMail(adminMailOptions);
        console.log('Admin notification email sent');

        res.status(200).json({ message: 'Form submitted successfully' });
    } catch (error) {
        console.error('Error submitting form:', error);
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

// Drop and recreate the table, then start the server¯
sequelize.sync({ force: true }).then(() => {
    console.log('Database synced');
    app.listen(port, () => {
        console.log(`Server is running on http://localhost:${port}`);
    });
}).catch(err => {
    console.error('Unable to sync the database:', err);
});
